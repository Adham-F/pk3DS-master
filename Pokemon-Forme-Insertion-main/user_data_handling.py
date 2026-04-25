import csv
from tkinter.filedialog import asksaveasfilename, askopenfilename
from utilities import *
from file_handling import *
from functools import reduce

def binary_file_to_array(file_path: str) -> list[int]:
    with open(file_path, "r+b") as f:
        f.seek(0, os.SEEK_END)
        file_end = f.tell()
        f.seek(0, 0)
        return(list(f.read(file_end)))

def deconstruct_GARC(bindata, poke_edit_data):
        #header:
        # 0x4 Header length (4 bytes)
        # 0x10 Data start (4 bytes)
        # 0x14 total file length (4 bytes)

        #then depends on version
        
        # V4
        # 0x18 largest file size (unpadded)

        # V6

        # 0x18 largest file size (with padding if it exists)
        # 0x1C largest file size (without padding, virtually always equal to the above for our purposes)
        # 0x20 Padding value (usually 0x4)

        #counting from end of whatever version you're in (so 0x4 = 0x1C in v4, 0x24 in v6)

        # 0x8 FAT0 header length (counting from 0x4)
        # 0xC, number of files (2 bytes)
        # from 0x10, 4 bytes per file, each one is 0x10 times file number (start from 0)
        

        #from end of above, 0x4 - header length
        # 0x8 - file count, then 
        # then for each file, 0x01 00 00 00, then offset start, offset end, and file length, offset counting first byte of first file as 0x0

        #finally, last magic word, then header length (0xC), then length of actual data (same as final offset end from previous section

        #get Fat0 offset
        FAT0_offset = 0
        if(poke_edit_data.game in {"XY", "ORAS"}):
           FAT0_offset = 0x1C
        else:
           FAT0_offset = 0x24
        
        
        FATB_offset = FAT0_offset + from_little_bytes_int(bindata[FAT0_offset + 0x4:FAT0_offset + 0x8])

        file_count = from_little_bytes_int(bindata[FAT0_offset + 0x8:FAT0_offset + 0xA])

        data_absolute_offset = from_little_bytes_int(bindata[0x10:0x14])


        output_array = []

        #0xC is start of the actual file location/length data.
        FATB_offset += 0xC

        #iterate over the files, pulling the length from the FATB data, each file gets its own array in temp
        for _ in range(file_count):
            
            #move data pointer to start of next file
            data_offset = data_absolute_offset + from_little_bytes_int(bindata[FATB_offset + 0x4:FATB_offset + 0x8])
            #print(data_offset)
            #get length of current file
            file_length = from_little_bytes_int(bindata[FATB_offset + 0xC:FATB_offset + 0x10])

            #append the file to a new entry in output array
            output_array.append(bindata[data_offset:data_offset + file_length])
            

            #the offset end is different than start + length because length is padded to multiple of 4.

            #move to next file in FATB data
            FATB_offset += 0x10

        return(output_array)

def reconstruct_GARC(poke_edit_data, GARC_name):
    
    match GARC_name:
        case "personal":
            #merges concatenated file for output
            out_file = poke_edit_data.personal + [reduce(lambda i, j: i+j, poke_edit_data.personal)]
        case "evolution":
            out_file = poke_edit_data.evolution
        case "levelup":
            out_file = poke_edit_data.levelup
        case "model":
            #merges with header for output
            out_file = [poke_edit_data.model_header] + poke_edit_data.model
            
    file_count = len(out_file)

    temp = [0x0]*0x1C
    FAT0_offset = 0

    #magic GARC
    temp[0:4] = [0x43, 0x52, 0x41, 0x47]

    #Endian
    temp[0x08:0xA] = [0xFF, 0xFE]

    #header length and Version
    if(poke_edit_data in {"XY", "ORAS"}):
        temp[0x4] = 0x1C
        temp[0xB] = 0x04
        FAT0_offset = 0x1C
    else:
        temp[0x4] = 0x24
        temp[0xB] = 0x06
        temp.extend([0]*8)
        FAT0_offset = 0x24

    #section count
    temp[0xC] = 0x4

    #FAT0 Header allocation
    temp.extend([0]*(0xC + 4*file_count))
    
    #Magic FAT0
    temp[FAT0_offset:FAT0_offset + 4] = [0x4F, 0x54, 0x41, 0x46]
    
    #FAT0 length
    temp[FAT0_offset + 0x4:FAT0_offset + 0x8] = from_int_little_bytes(file_count*4 + 0xC, 0x4)

    #file count
    temp[FAT0_offset + 0x8:FAT0_offset + 0xA] = from_int_little_bytes(file_count, 0x2)

    #padding
    temp[FAT0_offset + 0xA:FAT0_offset + 0xC] = [0xFF, 0xFF]

    #write FAT0 thing
    pointer = FAT0_offset + 0xC
    for x in range(file_count):
        temp[pointer:pointer + 4] = from_int_little_bytes(x * 0x10, 0x4)
        pointer += 0x4


    #allocate BFAT, 0xC for header, then 0x10 per file
    temp.extend([0]*(0xC + 0x10*file_count))
    #magic BFAT
    temp[pointer:pointer + 4] = [0x42, 0x54, 0x41, 0x46]

    pointer +=4

    #BFAT length
    temp[pointer:pointer + 4] = from_int_little_bytes(file_count*0x10 + 0xC, 0x4)

    pointer +=4

    #BFAT file count
    temp[pointer:pointer + 2] = temp[FAT0_offset + 0x8:FAT0_offset + 0xA]

    pointer += 4

    #before we write the BFAT blocks, add the FIMB header so we can write those blocks and actual files at once

    #this will point at end of file
    fimb_pointer = len(temp)

    temp.extend([0]*(0xC))
    
    #magic FIMB
    temp[fimb_pointer :fimb_pointer  + 4] = [0x42, 0x4D, 0x49, 0x46]
    
    fimb_pointer  += 4

    #FIMB header length (3 high bytes are zero)
    temp[pointer] = [0x0C]


    #need to update this with final offset below
    fimb_pointer  += 4


    data_pointer = len(temp)

    #update GARC header with data start

    temp[0x10:0x14] = from_int_little_bytes(data_pointer, 0x4)

    offset = 0
    biggest_size = 0
    biggest_size_padding = 0
    for file in out_file:
        
        #padding
        temp[pointer:pointer + 4] = [0x01, 0x00, 0x00, 0x00]

        #offset start
        temp[pointer + 4: pointer + 8] = from_int_little_bytes(offset, 0x4)

        length = len(file)
        
        
        biggest_size = max(length, biggest_size)
        padding = (length - 4) % 4
        biggest_size_padding = max(length + padding, biggest_size_padding)
        offset += length + padding

        #offset end. When there is padding to z bytes, those extra bytes are filled with 0xFF, are NOT counted in the length, but ARE counted in the end-address
        temp[pointer + 8: pointer + 0xC] = from_int_little_bytes(offset, 0x4)

        #length
        temp[pointer + 0xC: pointer + 0x10] = from_int_little_bytes(length, 0x4)


        #extend temp by length of file
        temp.extend([0]*length)
        #write file to location
        temp[data_pointer: data_pointer + length] = file

        if(padding != 0):
            temp.extend([0xFF]*padding)

        data_pointer += length + padding

        pointer += 0x10

    #write total length of files
    temp[fimb_pointer:fimb_pointer + 4] = from_int_little_bytes(offset, 0x4)

    #in GARC header, need to write file length, and largest file size (plus padded max and padding in gen 7)

    #only write largest file size at FAT0_offset - 4
    if(poke_edit_data in {"XY", "ORAS"}):
        temp[FAT0_offset - 0x4:FAT0_offset] = from_int_little_bytes(biggest_size, 0x4)
    
    #starting from FAT0_offset - 0xC:
    #max of 0x4 and max file size
    #max file size
    #padding (0x4)
    else:
        temp[FAT0_offset - 0xC:FAT0_offset - 0x8] = from_int_little_bytes(biggest_size_padding, 0x4)
        temp[FAT0_offset - 0x8:FAT0_offset - 0x4] = from_int_little_bytes(biggest_size, 0x4)
        temp[FAT0_offset - 0x4:FAT0_offset] = from_int_little_bytes(0x4, 0x4)

    #write total length of entire GARC
    temp[0x14:0x18] = from_int_little_bytes(len(temp), 0x4)


    return(temp)

def save_GARC(poke_edit_data, GARC_name: str):

    temp = reconstruct_GARC(poke_edit_data, GARC_name)

    match GARC_name:
        case "personal":
            file_path = poke_edit_data.personal_path
        case "evolution":
            file_path = poke_edit_data.evolution_path
        case "levelup":
            file_path = poke_edit_data.levelup_path
        case "model":
            file_path = poke_edit_data.model_path

    with open(file_path, "w+b") as f:
        f.write(bytes(temp))


#loads list of filenames in extracted GARC if it exists, otherwise return empty array
def load_GARC(poke_edit_data, garc_path, target, gameassert):

    if(os.path.exists(garc_path)):
        poke_edit_data.game = gameassert

        try:
            file_array = deconstruct_GARC(binary_file_to_array(garc_path), poke_edit_data)

            match poke_edit_data.game:
                case "XY":
                    poke_edit_data.max_species_index = 721
                case "ORAS":
                    poke_edit_data.max_species_index = 721
                case "SM":
                    poke_edit_data.max_species_index = 802
                case "USUM":
                    poke_edit_data.max_species_index = 807

            match target:
                case "Personal":
                    poke_edit_data.personal_path = garc_path

                    #delete compilation file
                    file_array.pop()

                    poke_edit_data.personal = file_array
                    poke_edit_data = update_species_list(poke_edit_data)
                case "Levelup":
                    poke_edit_data.levelup_path = garc_path
                    poke_edit_data.levelup = file_array

                case "Evolution":
                    poke_edit_data.evolution_path= garc_path
                    poke_edit_data.evolution = file_array
                case "Model":
                    poke_edit_data.model_path = garc_path
                    #pop model header into its own file
                    poke_edit_data.model_header = file_array.pop(0)
                    poke_edit_data.model = file_array
                    poke_edit_data = update_model_list(poke_edit_data)
        except Exception as e:
            print(e)
            return(poke_edit_data)

    else:
        print("Garc folder not found, unreadable, or empty")
    return(poke_edit_data)

def choose_GARC(poke_edit_data, target, gameassert):
    
    targetpath = ''
    #Evolution table has a fixed length per personal file, 0x30 in gen VI, 0x40 in gen VII
    #Similarly, the Personal file itself is 0x50 in gen VI, 0x54 in gen VII (additional bytes for "is regional forme" and Species-specific Z move)
    match gameassert:
        case "XY":
            poke_edit_data.evolution_table_length = 0x30
            poke_edit_data.personal_table_length = 0x50
            match target:
                case "Model":
                    targetpath = '0/0/7'
                case "Personal":
                    targetpath = '2/1/8'
                case "Levelup":
                    targetpath = '2/1/4'
                case "Evolution":
                    targetpath = '2/1/5'
            poke_edit_data.modelless_exists = False
        case "ORAS":
            poke_edit_data.evolution_table_length = 0x30
            poke_edit_data.personal_table_length = 0x50
            match target:
                case "Model":
                    targetpath = '0/0/8'
                case "Personal":
                    targetpath = '1/9/5'
                case"Levelup":
                    targetpath = '1/9/1'
                case"Evolution":
                    targetpath = '1/9/2'
            poke_edit_data.modelless_exists = False
        case "SM":
            poke_edit_data.evolution_table_length = 0x40
            poke_edit_data.personal_table_length = 0x54
            match target:
                case"Model":
                    targetpath = '0/9/3'
                case"Personal":
                    targetpath = '0/1/7'
                case"Levelup":
                    targetpath = '0/1/3'
                case"Evolution":
                    targetpath = '0/1/4'
            poke_edit_data.modelless_exists = False
        case "USUM":
            poke_edit_data.evolution_table_length = 0x40
            poke_edit_data.personal_table_length = 0x54
            match target:
                case"Model":
                    targetpath = '0/9/4'
                case"Personal":
                    targetpath = '0/1/7'
                case"Levelup":
                    targetpath = '0/1/3'
                case"Evolution":
                    targetpath = '0/1/4'
        case "Select Game":
               print("Error: Game not set")
               return



    folder_path = askopenfilename(title='Select ' + target + ' GARC, a/' + targetpath)
    poke_edit_data = load_GARC(poke_edit_data, folder_path, target, gameassert)
    
    return(poke_edit_data)


#called if the game files have more (inserted) formes than the CSV file.
def rebuild_csv(poke_edit_data):

    #we will rebuild the csv file in this. initialize with the first row, the empty Personal file
    temp_csv_file = [['Egg/Blank', '',0,0,'NA']]
        
    #Now build an array where the jth entry is the number of models for the (j-1)th Pokemon species (e.g. 0 is Bulby). Last entry is the 1 model for egg.
    species_model_count = []
    
            
    for x in range(poke_edit_data.max_species_index + 1):
        #the third byte is how many models that species has.
        species_model_count.append(poke_edit_data.model_header[4*x + 2])
    
    #row 0 has the empty personal file, skip to next before first iteration
    row_index = 0
    model_index = 0


    for current_base_species in range(1, poke_edit_data.max_species_index + 1):
        #skip the empty personal line
        #grab the rows from the loaded csv that match the currently loaded base species
        species_indices_from_csv = find_rows_with_column_matching(poke_edit_data.master_list_csv, 2, current_base_species)

        #grab the number of personal and model instances for the current Pokemon from the loaded csv
        temp_personal_instance_csv = 0
        temp_model_instance_csv = 0
        
        for row_index in species_indices_from_csv:
            if(poke_edit_data.master_list_csv[row_index][3] != ''):
                temp_personal_instance_csv += 1
            if(poke_edit_data.master_list_csv[row_index][4] != ''):
                temp_model_instance_csv += 1

        #grab the number of personal files for this Pokemon from the GARC
        temp_personal_pointer_garc = from_little_bytes_int(poke_edit_data.personal[current_base_species][0x1C:0x1E])

         #if the pointer is 0 but the forme count is >1, then only 1 personal file
        if(temp_personal_pointer_garc in {'0', '', 0, 0x0}):
            temp_personal_instance_garc = 0x1
        else:
            temp_personal_instance_garc = from_little_bytes_int(poke_edit_data.personal[current_base_species][0x20])

        missing_personal_count = temp_personal_instance_garc - temp_personal_instance_csv
        missing_model_count = species_model_count[current_base_species - 1] - temp_model_instance_csv
        
        '''if(missing_personal_count != 0 or missing_personal_count != 0):
            print(poke_edit_data.master_list_csv[species_indices_from_csv[0]])
            print(missing_personal_count, temp_personal_instance_garc, temp_personal_instance_csv)
            print(missing_model_count, species_model_count[current_base_species - 1] , temp_model_instance_csv)'''


        #just append the rows in the CSV
        for row in species_indices_from_csv:
            temp_csv_file.append(poke_edit_data.master_list_csv[row])
            row_index += 1
            #check for personal formes sharing model that haven't been fixed and don't write a model index number
            if(not(temp_personal_instance_csv > temp_model_instance_csv)):
                #update model index number
                temp_csv_file[row][4] = row
                model_index += 1
                
        #both the same, no changes here
        if(missing_personal_count == 0 and missing_model_count == 0):
            pass
        #CSV has additions and deletions from game in both directions in either Model or Personal
        elif(missing_personal_count < 0 or missing_model_count < 0):
            print('Problem detected at row', row_index)
            print('CSV has additons AND deletions compared to GARC. Please refresh the CSV and try again.')
            return
        #at least one of model and Personal have more entries. 
        else:
            rows_to_add = max(missing_personal_count, missing_model_count)
            #get base species name
            temp_name = poke_edit_data.master_list_csv[species_indices_from_csv[0]][0]
            for x in range(rows_to_add):
                
                if(missing_personal_count > x):
                    temp_forme_personal_location = temp_personal_pointer_garc + (temp_personal_instance_csv - 1) + x
                #keep things in line if there's a model file without a personal
                else:
                    temp_forme_personal_location = ''
                    temp_personal_pointer_garc -= 1
                if(missing_model_count > x):
                    temp_model_index = model_index
                    model_index += 1
                else:
                    temp_model_index = ''
                temp_csv_file.append([temp_name, temp_personal_instance_csv + x, current_base_species, temp_forme_personal_location, temp_model_index])
                
                row_index += 1

    #drop in the egg model last
    temp_csv_file.append(['Egg', '','NA','',model_index])
    
    #update the model indices
    for row_number, row in enumerate(temp_csv_file):
        if(row_number == 0):
            row[4] = ''
        else:
            row[4] = row_number - 1

    poke_edit_data.master_list_csv = temp_csv_file.copy()
    
    write_CSV(poke_edit_data)

    return(poke_edit_data)
             
#loads the data from the filepath in the class data structure to the correct variables
def load_names_from_CSV(poke_edit_data, just_wrote = False):
    
    
    temp_base_species_list =  []
    temp_master_formes_list = []
    temp_model_source_list = []
    temp_loaded_csv = []

    try:
        with open(poke_edit_data.csv_pokemon_list_path, newline = '', encoding='utf-8-sig') as csvfile:
            reader_head = csv.reader(csvfile, dialect='excel', delimiter=',')
        
            #load csv into an array      
            loaded_csv_file = list(reader_head)
        
            #check to see if older version from before saving the model header bytes and removes the header row
            try:
                has_bitflag = False
                if len(loaded_csv_file) > 0 and len(loaded_csv_file[0]) > 0:
                    # Check if the first row is actually the header before popping
                    if loaded_csv_file[0][0] == 'Base Index':
                        header_row = loaded_csv_file.pop(0)
                        if len(header_row) > 14 and header_row[14] == 'Model Bitflag 1':
                            has_bitflag = True
            except Exception as e:
                print('Error when trying to check for model bitflag in CSV, ', e)
                has_bitflag = False
            
        
            #We need to find the max personal file index since that's not in order with the structure of the models
            personal_max_temp = max_of_column(loaded_csv_file, 1)
            #and now give the formes_list table the right size:
            #has max index + 1 entries, because there is both a 0th and max indexth entry                
            for _ in range(personal_max_temp + 1):
                temp_master_formes_list.append('')
                
            for data_rows in loaded_csv_file:
                
                #build the actual csv file

                temp_row = [data_rows[3], data_rows[4], '', '', '', 0, 0]

                try:
                    temp_row[2] = int(data_rows[0])
                except:
                    temp_row[2] = data_rows[0]
                try:
                    temp_row[3] = int(data_rows[1])
                except:
                    temp_row[3] = data_rows[1]
                try:
                    temp_row[4] = int(data_rows[2])
                except:
                    temp_row[4] = data_rows[2]
                
                #load the two bytes from the end of the model table thing
                if(has_bitflag):
                    if(has_bitflag and not(poke_edit_data.modelless_exists and temp_row[3] == 975 and poke_edit_data.game == 'USUM')):
                        temp_row[5] = data_rows[14]
                        temp_row[6] = data_rows[15]

                temp_loaded_csv.append(temp_row)
                
                #print(temp_row)
                #build the underlying species, forme, and model file lists
                #if personal index is the same as the nat dex number, is the base forme, so append the species name
                if(data_rows[0].isdigit() and data_rows[1].isdigit() and data_rows[0] == data_rows[1]):
                    temp_base_species_list.append(data_rows[3])
                #if the forme-name slot is non-empty, will be used at least once following this, if it empty need to not have dangling seperator
                if(data_rows[4] != ""):
                    temp_forme_name = data_rows[3] + " - " + data_rows[4]
                else:
                    temp_forme_name = data_rows[3]
                    
                #if the personal index is NOT (empty or NA), then write the species name + forme name to the formes list
                if(not(data_rows[1] in {"", "NA"})):
                    temp_master_formes_list[int(data_rows[1])]= temp_forme_name
                    
                #if the model index is NOT empty or NA, then write the species name + forme name to the formes list. This should only happen for the very first entry, so report an error if that happens
                if(not(data_rows[2] in {"", "NA"})):
                    temp_model_source_list.append(temp_forme_name)
                elif(data_rows[2].isdigit() and int(data_rows[2]) > 0):
                    print('Entry without unique model file detected at Species Index-Personal Index-Name:' + data_rows[0] + '-' + data_rows[1] + '-' + data_rows[3] + '-' + data_rows[4])
                    

                #print(data_rows)
        #series of checks to see if it is the case that the loaded CSV arrays are shorter than the default-created ones (in which case assume we just created/refreshed the CSV, or loaded the wrong thing), or is longer (in which case the CSV has more entries than the game files and something is terribly wrong)
        #only do this if we loaded, not if we just insert a Pokemon        
        
        #if CSV is old version, need to load bitflags from file
        if(not has_bitflag):
            #move to start of bitflags, which is at 0x4*max nat dex
            start_offset = 4*(poke_edit_data.max_species_index + 1)
                    
            #monotone increasing on both sides, so each bitflag bytepair goes to the next row with a model
            loaded_csv_row = 1

            for offset in range(start_offset, len(poke_edit_data.model_header) - 2, 2):
                        
                #check to see if current CSV row has a model index (should only skip Dusk Rockruff in USUM before fix is applied)
                        
                while True:
                    if(isinstance(temp_loaded_csv[loaded_csv_row][4], int)):
                        temp_loaded_csv[loaded_csv_row][5:7] = poke_edit_data.model_header[offset:offset + 2]
                        loaded_csv_row += 1
                        break
                    loaded_csv_row += 1
        
        if(just_wrote):
            
            species_check = len(temp_base_species_list) - len(poke_edit_data.base_species_list)
            forme_check = len(temp_master_formes_list) - len(poke_edit_data.master_formes_list)
            model_check = len(temp_model_source_list) - len(poke_edit_data.model_source_list)

            if(species_check < 0):
                print('The loaded CSV has fewer Pokemon base species than your game files. Something is very probably wrong unless you have successfully added new species to the game (in which case please submit a bug report so I can update). The Pokemon base species entries read from the CSV have NOT been loaded.')
                return(poke_edit_data)
            elif(species_check > 0):
                print('The loaded CSV has more Pokemon base species than your game files. Something is very probably wrong. Please recheck your game files, the csv itself, and your settings. The Pokemon base species entries read from the CSV have NOT been loaded.')
                return(poke_edit_data)
            elif(species_check == 0):
                print('Loading Pokemon Species List from CSV')
                poke_edit_data.base_species_list = temp_base_species_list.copy()
            
            if(forme_check < 0):
                print('The loaded CSV might have fewer Forme entries than your game files. Unless you have not previously selected or initialized a csv for your game, or for whatever reason refreshed it to default, something might be wrong. Please double-check your file selections and settings. Will rebuild CSV file in memory from game files, if this is wrong, please exit without saving')
                poke_edit_data = rebuild_csv(poke_edit_data)
            elif(forme_check > 0):
                print('The loaded CSV has more total Forme entries than your game files. Something is wrong. Please double-check your file selections and settings. The Forme entries read from the CSV have NOT been loaded.')
                return(poke_edit_data)
                    
            if(forme_check == 0):
                print('Loading Formes List from CSV')
                poke_edit_data.master_formes_list = temp_master_formes_list.copy()
            
            if(model_check < 0):
                print('The loaded CSV might have fewer total Model entries than your game files. Unless you have not previously selected or initialized a csv for your game, or for whatever reason refreshed it to default, something might be wrong. Please double-check your file selections and settings. Will attempt to update.')
                poke_edit_data = rebuild_csv(poke_edit_data)
                poke_edit_data = load_names_from_CSV(poke_edit_data, True)
            elif(model_check > 0):
                print('The loaded CSV has more total Model entries than your game files. Something is wrong. Please double-check your file selections and settings. The Model entries read from the CSV have NOT been loaded.')
            if(model_check == 0):
                print('Loading Model List from CSV')
                poke_edit_data.model_source_list = temp_model_source_list.copy()
        else:
            poke_edit_data.base_species_list = temp_base_species_list.copy()
            poke_edit_data.master_formes_list = temp_master_formes_list.copy()
            poke_edit_data.model_source_list = temp_model_source_list.copy()
    except Exception as e:
       print(e)
       return(poke_edit_data)
    

    
    poke_edit_data.master_list_csv = temp_loaded_csv.copy()

    return(poke_edit_data)

#just asks for the path and calls the write-csv-to-the-right-part-of-the-class-data-structure program
def user_prompt_load_CSV(poke_edit_data, target):

    poke_edit_data.csv_pokemon_list_path = askopenfilename(title='Select ' + target + ' CSV')
    

    poke_edit_data = load_names_from_CSV(poke_edit_data)
    

    return(poke_edit_data)



def write_CSV(poke_edit_data, csv_path = ''):

    #use saved config path if nothing set
    if(csv_path == ''):
        csv_path = poke_edit_data.csv_pokemon_list_path
    else:
        poke_edit_data.csv_pokemon_list_path = csv_path

    try:
        with open(csv_path, 'w', newline='', encoding='utf-8-sig') as csvfile:
            writer_head = csv.writer(csvfile, dialect='excel', delimiter=',')
            #write the header line
            writer_head.writerow (['Base Index', 'Personal Index', 'Model Index', 'Species', 'Forme', 'Model', 'Texture', 'Shiny_Texture', 'Greyscale_Texture', 'Battle_Animations', 'Refresh_Animations', 'Movement_Animations', 'Lip_Animations', 'Empty', 'Model Bitflag 1', 'Model Bitflag 2', 'Portrait', 'Shiny_Portrait', 'Icon'])
            
            if(poke_edit_data.game in {'SM', 'USUM'}):
                model_file_start = 1
                model_file_count = 9                
            else:
                model_file_count = 8
                
                if(poke_edit_data.game == 'XY'):
                    model_file_start = 4
                else:
                    model_file_start = 3
                
            #print(len(poke_edit_data.master_list_csv))
            #iterate over the names in the model source list
            #write species index to column A, personal file index to B, model index to C, species name to D, forme to E, then model/texture/animaiton filenames in 6 starts at 4, 3, 1 for XY, ORAS, SMUSUM
            for enum, pokemon_instance in enumerate(poke_edit_data.master_list_csv):
                if(enum == 0):
                    writer_head.writerow ([pokemon_instance[2], pokemon_instance[3], pokemon_instance[4], pokemon_instance[0], pokemon_instance[1]] + ['' for x in range(model_file_count)] + ['', ''])
                else:
                    #print([pokemon_instance[2], pokemon_instance[3], pokemon_instance[4], pokemon_instance[0], pokemon_instance[1]] + [(enum - 1)*model_file_count + x + model_file_start for x in range(model_file_count)])
                    if(poke_edit_data.game in {'SM', 'USUM'}):
                        writer_head.writerow ([pokemon_instance[2], pokemon_instance[3], pokemon_instance[4], pokemon_instance[0], pokemon_instance[1]] + [(enum - 1)*model_file_count + x + model_file_start for x in range(model_file_count)] + [pokemon_instance[5], pokemon_instance[6]])
                    else:
                        writer_head.writerow ([pokemon_instance[2], pokemon_instance[3], pokemon_instance[4], pokemon_instance[0], pokemon_instance[1]] + [(enum - 1)*model_file_count + x + model_file_start for x in range(model_file_count)] + '' + [pokemon_instance[5], pokemon_instance[6]])
                        
    #don't do anything and proceed as usual if none exists, print error message
    except Exception as e:
        print(e, 'If this error message is thrown and the CSV has all the Pokemon in it, everything is fine, not sure why this error is happening')#'Selected CSV file is open in another program. Please close it and try again')
    
    
    #print('after write')
    #for pokemon_instance in poke_edit_data.master_list_csv:
    #    print(pokemon_instance)
    return(poke_edit_data)

def user_prompt_write_CSV(poke_edit_data, target, gamesassert = ''):

    if(gamesassert != ''):
        poke_edit_data.game = gamesassert

    if(poke_edit_data.model == []):
        print('Please load the GARCs before creating CSV')
        return(poke_edit_data)

    flags_offset = 4*(1 + poke_edit_data.max_species_index)
    
    for row in poke_edit_data.master_list_csv:
        if(row[4] != '' and row[4] != 'NA'):
            if(row[4] >= 0 and len(row) == 5):
                temp = [poke_edit_data.model_header[flags_offset], poke_edit_data.model_header[flags_offset + 1]]
                flags_offset += 2
            else:
                temp = ['', '']
        else:
            temp = ['', '']
        row.extend(temp)
    
    write_CSV(poke_edit_data, asksaveasfilename(title='Select ' + target + ' CSV', defaultextension='.csv',filetypes= [('CSV','.csv')]))
    
    return(poke_edit_data)

def create_refresh_CSV(poke_edit_data, gameassert):
    

    #update game in data structure (in case this is the first thing selected)
    if(gameassert in {'XY', 'ORAS', 'SM', 'USUM'}):
        poke_edit_data.game = gameassert


    if(not(poke_edit_data.game in {'XY', 'ORAS', 'SM', 'USUM'})):
        print('Please select supported game')
        return
    match poke_edit_data.game:
        case 'XY':
            poke_edit_data.master_list_csv = xy_master_list_csv.copy()
        case 'ORAS':
            poke_edit_data.master_list_csv = oras_master_list_csv.copy()
        case 'SM':
            poke_edit_data.master_list_csv = oras_master_list_csv.copy()
        case 'USUM':
            poke_edit_data.master_list_csv = usum_master_list_csv.copy()

    user_prompt_write_CSV(poke_edit_data, 'Pokemon Names and Files')
    poke_edit_data = load_names_from_CSV(poke_edit_data)

    return(poke_edit_data)

def load_game_cfg(poke_edit_data):
    
    game_cfg_path = askopenfilename(title='Select cfg file', defaultextension='.cfg',filetypes= [('config','.cfg')])

    '''read all the lines
    0 = game
    1 = Personal
    2 = Level
    3 = Evolution
    4 = Model/texture
    5 = list of modelles formes
    6 = all clear bool
    7 = portrait_1
    8 = portrait_2
    9 = extension
    10 = max species index
    11 = Names Table CSV
    '''
    
    cfg_desc = ["Game", "Personal path", "Levelup path", "Evolution path", "Pokemon Model/Texture path",'','Need to update Models','','','Sorted',"Max Species Index", "Names and Model File List CSV Path"]
 
    try:
        with open(game_cfg_path, "r") as cfg:
            cfg_array = [line.rstrip() for line in cfg]
    except Exception as e:
        print(e)
        return(poke_edit_data)
    try:
        poke_edit_data.game = cfg_array[0]
        poke_edit_data.personal_path = cfg_array[1]
        poke_edit_data.levelup_path = cfg_array[2]
        poke_edit_data.evolution_path = cfg_array[3]
        poke_edit_data.model_path = cfg_array[4]
        # = cfg_array[5]
        #check to avoid issues loading old files, defaults to True due to class construction if neither passes
        if(cfg_array[6] == 'True'):
            poke_edit_data.modelless_exists = True
        elif(cfg_array[6] == 'False'):
            poke_edit_data.modelless_exists = False
        
        #evolution = cfg_array[7]
        #evolution = cfg_array[8]
        if(cfg_array[9] == 'True'):
            poke_edit_data.sorted = True
        else:
            poke_edit_data.sorted = False


        poke_edit_data.max_species_index = cfg_array[10]
        poke_edit_data.csv_pokemon_list_path = cfg_array[11]
    except:
        print('Config file missing lines, loaded what was there')
        
    try:
        print('Data loaded as follows:')
        for x in range(len(cfg_desc)):
            if(cfg_desc[x] != ''):
                print(cfg_desc[x] + ': ' + str(cfg_array[x]))
    except:
        print('Missing lines from config')
    print('\n')

    poke_edit_data = load_GARC(poke_edit_data, poke_edit_data.personal_path, "Personal", poke_edit_data.game)
    poke_edit_data = load_GARC(poke_edit_data, poke_edit_data.levelup_path, "Levelup", poke_edit_data.game)
    poke_edit_data = load_GARC(poke_edit_data, poke_edit_data.evolution_path, "Evolution", poke_edit_data.game)
    poke_edit_data = load_GARC(poke_edit_data, poke_edit_data.model_path, "Model", poke_edit_data.game)
    poke_edit_data = load_names_from_CSV(poke_edit_data)
        
    print('Sorted: ', poke_edit_data.sorted, '\n')
    print("Number of Species:", len(poke_edit_data.base_species_list), "(not including the Egg)")
    print("Number of Personal Entries:", len(poke_edit_data.master_formes_list) - 1, "(not including the Egg)")
    print("Number of Model Entries:", len(poke_edit_data.model_source_list))
    '''except:
        print('Selection missing or invalid')'''

    return(poke_edit_data)
    
def save_game_cfg(poke_edit_data, game_set = ''):
 
    game_cfg_path = asksaveasfilename(title='Select location to save cfg file', defaultextension='.cfg',filetypes= [('config','.cfg')])
    
    if(game_set != '' and poke_edit_data.game == ''):
        poke_edit_data.game = game_set
        
    
    if(poke_edit_data.modelless_exists and poke_edit_data.game == 'USUM'):
        temp_modelless = 'True'
    else:
        temp_modelless = 'False'
        poke_edit_data.modelless_exists = False
    
    #catch for user not having set their CSV path
    if(poke_edit_data.csv_pokemon_list_path == ''):
        user_prompt_write_CSV(poke_edit_data, 'Pokemon Names and Files')

            

    try:
        with open(game_cfg_path, "w") as cfg:
            cfg.write(poke_edit_data.game + '\n')
            cfg.write(poke_edit_data.personal_path + '\n')
            cfg.write(poke_edit_data.levelup_path + '\n')
            cfg.write(poke_edit_data.evolution_path + '\n')
            cfg.write(poke_edit_data.model_path + '\n')
            cfg.write('\n')
            cfg.write(temp_modelless + '\n')
            cfg.write('\n')#evolution = cfg_array[7]
            cfg.write('\n')#evolution = cfg_array[8]
            cfg.write(str(poke_edit_data.sorted) + '\n')
            cfg.write(str(poke_edit_data.max_species_index) + '\n')
            cfg.write(poke_edit_data.csv_pokemon_list_path)
        print('Config file saved to ' + game_cfg_path)
        write_CSV(poke_edit_data)
        print('Names and Model File List CSV saved to ' + poke_edit_data.csv_pokemon_list_path)
    except Exception as e:
            print(e, 'save game error')
