using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using pk3DS.Core.Structures;
using pk3DS.Core;

namespace pk3DS.WinForms
{
    public static class ModernLearnsetImporter
    {
        /// <summary>
        /// Parse learnsets.txt into a dictionary: speciesKey -> list of (moveName, level) for the given gen.
        /// Uses a line-by-line state machine to avoid regex cross-contamination between species.
        /// </summary>
        private static Dictionary<string, List<(string Move, int Level)>> ParseLearnsets(string content, int genIdentifier)
        {
            var result = new Dictionary<string, List<(string, int)>>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(content)) return result;

            string genTag = $"{genIdentifier}L";
            var moveRegex = new Regex($@"['""]?([\w\-]+)['""]?\s*:\s*\[[^\]]*""{genTag}(\d+)""[^\]]*\]", RegexOptions.Compiled);

            // Clean up content to handle different line endings
            content = content.Replace("\r\n", "\n").Replace("\r", "\n");
            
            // We look for species blocks by finding keys at the start of a line (or start of file)
            // Showdown format: speciesName: {
            string[] lines = content.Split('\n');
            string currentSpecies = null;
            StringBuilder currentBlock = new StringBuilder();
            bool inSpecies = false;
            int bracketCount = 0;

            foreach (var line in lines)
            {
                string trimmed = line.Trim();
                if (trimmed.Length == 0) continue;

                if (!inSpecies)
                {
                    var specMatch = Regex.Match(trimmed, @"^['""]?([\w\-]+)['""]?\s*:\s*\{");
                    if (specMatch.Success)
                    {
                        currentSpecies = specMatch.Groups[1].Value.ToLowerInvariant().Replace("-", "").Replace("'", "");
                        currentBlock.Clear();
                        currentBlock.AppendLine(line);
                        inSpecies = true;
                        bracketCount = 1;
                    }
                    continue;
                }

                currentBlock.AppendLine(line);
                bracketCount += line.Count(c => c == '{');
                bracketCount -= line.Count(c => c == '}');

                if (bracketCount <= 0)
                {
                    ProcessBlock(currentSpecies, currentBlock.ToString(), genTag, moveRegex, result);
                    inSpecies = false;
                    currentSpecies = null;
                }
            }
            
            // Final species
            if (inSpecies && currentSpecies != null)
                ProcessBlock(currentSpecies, currentBlock.ToString(), genTag, moveRegex, result);

            return result;
        }

        private static void ProcessBlock(string species, string block, string genTag, Regex moveRegex, Dictionary<string, List<(string, int)>> result)
        {
            int lsStart = block.IndexOf("learnset:", StringComparison.OrdinalIgnoreCase);
            if (lsStart == -1) return;

            var moves = new List<(string, int)>();
            var matches = moveRegex.Matches(block, lsStart);
            foreach (Match m in matches)
            {
                if (int.TryParse(m.Groups[2].Value, out int lv))
                    moves.Add((m.Groups[1].Value, lv));
            }

            if (moves.Count > 0)
                result[species] = moves;
        }

        public static void ApplyModernLearnsets(byte[][] files, string[] movelist, string[] specieslist, int genIdentifier, int[] baseForms = null)
        {
            if (files == null || movelist == null || specieslist == null) return;
            string content = GetLearnsetContent();
            if (string.IsNullOrEmpty(content))
            {
                WinFormsUtil.Error("Could not find learnsets.txt in project root or app directory.\nPlease ensure the file is present.");
                return;
            }

            var learnsetDB = ParseLearnsets(content, genIdentifier);
            var moveLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 1; i < movelist.Length; i++)
            {
                string norm = NormalizeName(movelist[i]);
                if (!moveLookup.ContainsKey(norm)) moveLookup[norm] = i;
            }

            int successCount = 0;
            for (int i = 1; i < files.Length; i++)
            {
                byte[] rawInput = files[i];
                if (rawInput == null || rawInput.Length <= 4) continue;

                int sIndex = (baseForms != null && i < baseForms.Length && i > Main.Config.MaxSpeciesID) ? baseForms[i] : i;
                if (sIndex < 0 || sIndex >= specieslist.Length) continue;
                
                string speciesKey = RemapSpeciesKey(NormalizeName(specieslist[sIndex]));
                
                // Form detection
                if (i >= 0 && i < specieslist.Length && i > Main.Config.MaxSpeciesID)
                {
                    string fullName = specieslist[i].ToLowerInvariant();
                    if (fullName.Contains("alola")) speciesKey += "alola";
                    else if (fullName.Contains("galar")) speciesKey += "galar";
                }

                if (!learnsetDB.TryGetValue(speciesKey, out var learnList)) continue;

                var newMoves = new List<int>();
                var newLevels = new List<int>();

                foreach (var (moveName, lv) in learnList)
                {
                    if (moveLookup.TryGetValue(NormalizeName(moveName), out int moveIdx))
                    {
                        newMoves.Add(moveIdx);
                        newLevels.Add(lv);
                    }
                }

                if (newMoves.Count > 0)
                {
                    // Deduplicate and Sort: Sort by level, then by original index to preserve intended sequence
                    var sorted = Enumerable.Range(0, newMoves.Count)
                        .OrderBy(idx => newLevels[idx])
                        .ThenBy(idx => idx) // Preserve stable order
                        .Select(idx => new { Move = newMoves[idx], Level = newLevels[idx] })
                        .Distinct().ToList();

                    var pkm = new Learnset6(rawInput)
                    {
                        Moves = sorted.Select(x => x.Move).ToArray(),
                        Levels = sorted.Select(x => x.Level).ToArray()
                    };
                    files[i] = pkm.Write();
                    successCount++;
                }
            }
            WinFormsUtil.Alert($"Modern update complete!\nSuccessfully applied Gen {genIdentifier} learnsets to {successCount} Pokémon.");
        }

        // Lazy-cached learnset content string to avoid re-reading the resource on every species switch
        private static string _cachedContent = null;

        private static string GetLearnsetContent()
        {
            if (_cachedContent != null) return _cachedContent;
            try
            {
                // Prioritize the specific path provided by the user in the repo root
                string repoPath = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\..\learnsets.txt"));
                if (File.Exists(repoPath)) return _cachedContent = File.ReadAllText(repoPath);

                string rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "learnsets.txt");
                if (File.Exists(rootPath)) return _cachedContent = File.ReadAllText(rootPath);

                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("pk3DS.WinForms.Resources.learnsets.txt");
                if (stream != null) using (var reader = new StreamReader(stream)) _cachedContent = reader.ReadToEnd();
                else
                {
                    string fallback = Path.Combine(Application.StartupPath, "learnsets.txt");
                    if (File.Exists(fallback)) _cachedContent = File.ReadAllText(fallback);
                }
            }
            catch { }
            return _cachedContent ?? "";
        }

        /// <summary>
        /// Returns a normalizedMoveName -&gt; level dictionary for the given species and generation.
        /// Used by LevelUpEditor to fill Gen8/Gen9 comparison columns.
        /// </summary>
        public static Dictionary<string, int> GetMoveLevelMap(int entryIndex, string[] specieslist, int[] baseForms, int genIdentifier)
        {
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            string content = GetLearnsetContent();
            if (string.IsNullOrEmpty(content)) return result;

            int sIndex = (baseForms != null && entryIndex < baseForms.Length && entryIndex > Main.Config.MaxSpeciesID) ? baseForms[entryIndex] : entryIndex;
            string speciesKey = RemapSpeciesKey(NormalizeName(specieslist[sIndex]));
            if (entryIndex > Main.Config.MaxSpeciesID)
            {
                string fullName = specieslist[entryIndex].ToLowerInvariant();
                if (fullName.Contains("alola")) speciesKey += "alola";
                else if (fullName.Contains("galar")) speciesKey += "galar";
                else if (fullName.Contains("mega")) speciesKey += "mega";
            }

            var db = ParseLearnsets(content, genIdentifier);
            if (!db.TryGetValue(speciesKey, out var list)) return result;

            foreach (var (moveName, lv) in list)
                result[NormalizeName(moveName)] = lv;

            return result;
        }

        public static string NormalizeName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            return name.ToLowerInvariant()
                .Replace(" ", "").Replace("-", "").Replace(".", "")
                .Replace(":", "").Replace("'", "").Replace("\u2019", "")
                .Replace("%", "").Replace("(", "").Replace(")", "");
        }

        private static string RemapSpeciesKey(string key)
        {
            return key switch
            {
                "farfetchd"  => "farfetchd",
                "mrmime"     => "mrmime",
                "mimejr"     => "mimejr",
                "mimetwo"    => "mimejr",
                "typenull"   => "typenull",
                "jangmoo"    => "jangmoo",
                "hakamoo"    => "hakamoo",
                "kommoo"     => "kommoo",
                "tapukoko"   => "tapukoko",
                "tapulele"   => "tapulele",
                "tapubulu"   => "tapubulu",
                "tapufini"   => "tapufini",
                "flabebe"    => "flabebe",
                "meowstic"   => "meowstic",
                "aegislash"  => "aegislash",
                "pumpkaboo"  => "pumpkaboo",
                "gourgeist"  => "gourgeist",
                "hoopa"      => "hoopa",
                "zygarde"    => "zygarde",
                "oricorio"   => "oricorio",
                "minior"     => "minior",
                "wishiwashi" => "wishiwashi",
                _            => key
            };
        }
    }
}
