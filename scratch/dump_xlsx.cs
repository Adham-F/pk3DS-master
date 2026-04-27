using System;
using pk3DS.Core.Modding;
using System.Collections.Generic;

class Program {
    static void Main() {
        string path = @"c:\Users\fulto\Downloads\3DS\pk3DS-master\ARM Functions\Shop Custom Relocation Patches.xlsx";
        var rows = XlsxResearchParser.ReadSheet(path, "Custom Relocation Patches");
        Console.WriteLine("Rows: " + rows.Count);
        foreach (var row in rows) {
            foreach (var kvp in row) {
                Console.Write($"{kvp.Key}: {kvp.Value} | ");
            }
            Console.WriteLine();
        }
    }
}
