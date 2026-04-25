from tkinter.filedialog import asksaveasfile
from my_constants import *
from user_data_handling import *
from file_handling import *


def save_and_refresh_GARCs(poke_edit_data):
    #save the GARCs
    save_GARC(poke_edit_data, 'personal')
    save_GARC(poke_edit_data, 'evolution')
    save_GARC(poke_edit_data, 'levelup')
    save_GARC(poke_edit_data, 'model')
    print('GARC files written\n')
    
    poke_edit_data.run_model_later = False
    poke_edit_data = update_model_list(poke_edit_data)
    poke_edit_data = update_species_list(poke_edit_data)
    print('Internal tables updated with changes' + '\n')
    return(poke_edit_data)

#Sorts Forme file order of Personal, Evolution, and Levelup Garc folders in order to allow for new formes of existing multi-formed Pokemon to be added
def resort_file_structure(poke_edit_data):
    
    forme_index = poke_edit_data.max_species_index + 1
    repoint_alts_array = []
    
    # Initialize as copies of the original data to preserve base forms
    temp_personal = poke_edit_data.personal.copy()
    temp_levelup = poke_edit_data.levelup.copy()
    temp_evolution = poke_edit_data.evolution.copy()

    #build table for sorting
    for row_number, row in enumerate(poke_edit_data.master_list_csv):
        #ensure that we are looking at a real personal entry
        if(isinstance(row[2], int) and isinstance(row[3], int)):
            
            #if base forme index == personal index, look for any alt formes in personal, and write them to next slots.
            if(row[2] == row[3]):
                #get forme count and old pointer
                forme_count = poke_edit_data.personal[row[2]][0x20]
                old_forme_pointer = from_little_bytes_int(poke_edit_data.personal[row[2]][0x1C:0x1E])

                #if there are more personal files
                if(old_forme_pointer != 0 and forme_count > 1):
                    
                    #update with new pointer in both arrays
                    poke_edit_data.personal[row[2]][0x1C:0x1E] = from_int_little_bytes(forme_index, 0x2)
                    temp_personal[row[2]][0x1C:0x1E] = from_int_little_bytes(forme_index, 0x2)
                    
                    #then for each of the alts, update the pointer, then move all 3 files to the new array
                    for x in range(forme_count - 1):
                        target_index = old_forme_pointer + x
                        
                        # Ensure main data arrays are large enough using list padding
                        while len(poke_edit_data.personal) <= target_index:
                            poke_edit_data.personal.append([0] * len(poke_edit_data.personal[0]))
                        while len(poke_edit_data.levelup) <= target_index:
                            poke_edit_data.levelup.append([0] * len(poke_edit_data.levelup[0]))
                        while len(poke_edit_data.evolution) <= target_index:
                            poke_edit_data.evolution.append([0] * len(poke_edit_data.evolution[0]))
                            
                        # Ensure temp arrays are large enough using list padding
                        while len(temp_personal) <= forme_index:
                            temp_personal.append([0] * len(poke_edit_data.personal[0]))
                            temp_levelup.append([0] * len(poke_edit_data.levelup[0]))
                            temp_evolution.append([0] * len(poke_edit_data.evolution[0]))

                        # Update pointer
                        poke_edit_data.personal[target_index][0x1C:0x1E] = from_int_little_bytes(forme_index, 0x2)

                        # Move to temp
                        temp_personal[forme_index] = poke_edit_data.personal[target_index]
                        temp_levelup[forme_index] = poke_edit_data.levelup[target_index]
                        temp_evolution[forme_index] = poke_edit_data.evolution[target_index]

                        #base forme, old forme index, new forme index
                        repoint_alts_array.append([row[2], target_index, forme_index])
                        
                        #increment forme index pointer
                        forme_index += 1
            else:
                for row_index,other_row in enumerate(repoint_alts_array):
                    #find the entry that has the same base forme and old forme index
                    if(other_row[0] == row[2] and other_row[1] == row[3]):
                        #update forme index
                        row[3] = other_row[2]
                        repoint_alts_array.pop(row_index)
                        break

    poke_edit_data.personal = temp_personal.copy()
    poke_edit_data.evolution = temp_evolution.copy()
    poke_edit_data.levelup = temp_levelup.copy()

    poke_edit_data.sorted = True
    save_and_refresh_GARCs(poke_edit_data)
    return(poke_edit_data)

def check_adding_without_models_works(poke_edit_data, base_form_index, new_forme_count):
    
    temp_forme_count_from_personal_file_count = 1
    explicit_forme_count = 0
    update_forme_count = 0
    #get the number of Personal files this pokemon actually has
    explicit_forme_count = poke_edit_data.personal[base_form_index][0x20]
    temp_pointer = from_little_bytes_int(poke_edit_data.personal[base_form_index][0x1C:0x1E])
    if(temp_pointer != 0):
        for offset in range(0, explicit_forme_count - 1):
            if(temp_pointer == from_little_bytes_int(poke_edit_data.personal[temp_pointer + offset][0x1C:0x1E])):
                temp_forme_count_from_personal_file_count += 1


    temp_model_count = poke_edit_data.model_header[4*(base_form_index - 1) + 2]
			

    #if the number of models is greater than or equal to the total number of personal file formes plus the number being inserted, we're good (e.g. inserting 1 for Venusaur, 2 personal filed (base and mega), but 3 model files (male base, female base, mega), so 3 >= 2 + 1), we are good to go. Flip the cosmetic female forme flag off if needed, and return true

    if(temp_model_count >= temp_forme_count_from_personal_file_count + new_forme_count):

        #need to figure out if the alt formes are including in forme count
                
        #explicit_forme_count is >= the number of existing unique data instances plus the ones being added, we don't need to increase the explicit forme count. Otherwise, we need to update that. Could just do a max but this is clearer and trivially more overhead
        if(explicit_forme_count >= temp_forme_count_from_personal_file_count + new_forme_count):
            update_forme_count = explicit_forme_count
        else:
            update_forme_count = temp_forme_count_from_personal_file_count + new_forme_count

		#unset the "female cosmetic forme" flag if set
        if poke_edit_data.model_header[4*(base_form_index - 1) + 3] in {3, 7}:
            poke_edit_data.model_header[4*(base_form_index - 1) + 3] = 5
        return(poke_edit_data, True, update_forme_count)
    else:
        print('There are ' + str(temp_model_count) + ' model files and ' + str(temp_forme_count_from_personal_file_count) + ' Personal files, aborting.')
        return(poke_edit_data, False, 0)

#handles actual editing and moving of files for forme insertion
#handles actual editing and moving of files for forme insertion
def add_new_forme_execute(poke_edit_data, base_form_index, new_forme_count, model_source_index, personal_source_index, levelup_source_index , evolution_source_index, def_model, skip_model_insertion, update_forme_count, defer_save=False):
    
    if(not(poke_edit_data.sorted)):
        print('Detected unsorted files, this might take a while.')
        poke_edit_data = resort_file_structure(poke_edit_data)
        return(poke_edit_data)

    #first unused file name
    start_location = 0
    
    #checks to ensure that the evolution, levelup, and Personal have the same number of files (including dummy 0th)
    if(not(len(poke_edit_data.evolution)  ==  len(poke_edit_data.levelup) == len(poke_edit_data.personal))):
        print('Mismatch in file counts:\n')
        print('Personal has:', len(poke_edit_data.personal), 'files')
        print('Levelup has:', len(poke_edit_data.levelup), 'files')
        print('Evolution has:', len(poke_edit_data.evolution), 'files')
        return(poke_edit_data)
    
    #This will hold the current file numbers of existing formes
    existing_formes_array = [base_form_index]

    total_formes = new_forme_count
    
    print('\n\nUpdating existing files for species ' + str(base_form_index))

    #pre-insertion number of formes
    old_forme_count = poke_edit_data.personal[base_form_index][0x20]
    #pre-insertion pointer to first alt forme
    old_forme_pointer = from_little_bytes_int(poke_edit_data.personal[base_form_index][0x1C:0x1E])

    #in this case, forme count is not going to change, since we are initializing unique data for existing cosmetic formes
    if(skip_model_insertion):
        total_formes = update_forme_count
    else:
        #Get the new total number of formes
        total_formes += old_forme_count
            
    #find existing formes
    if(old_forme_pointer > base_form_index):
        for x in range(old_forme_count - 1):

            #the forme pointer is the same for all instances, so if not equal, run out (this is to allow for Pokemon like Venusaur, that have both cosmetic and non-cosmetic formes)
            if(from_little_bytes_int(poke_edit_data.personal[old_forme_pointer + x][0x1C:0x1E]) == old_forme_pointer):
                existing_formes_array.append(old_forme_pointer + x)


    #we now have the file addresses of all the places we need to update for all but the model file
                    
    
    print("Copying new game data")
    #now initialize the newly added formes
    
    cur_forme_pointer = 0
    new_forme_pointer = 0

    #remember, .insert(x, y) places y at position x, at moves the thing there to later spot e.g. .insert(150, y) places y at Mewtwo's position, and Mewtwo is now at 151
    #if this Pokemon has no formes, find the first entry of the next species with alt formes, and insert there
    if(old_forme_count == 1):
        for index, pokemon in enumerate(poke_edit_data.personal):

            cur_forme_pointer = from_little_bytes_int(pokemon[0x1C:0x1E])

            #found first species with additional formes after this one, or current species if it has already
            if(index > base_form_index and cur_forme_pointer != 0):
                new_forme_pointer = cur_forme_pointer
                break
            else:
                #otherwise, we are appending to the very end
                new_forme_pointer = len(poke_edit_data.personal)
    else:
       #if more than just base forme, the last alt forme is at forme_pointer + (forme_count - 2), 1 taken out for base forme, and then the first alt forme is forme_pointer + 0. We need to insert from next thing, forme_pointer + 1, so just -1
        new_forme_pointer = old_forme_pointer + len(existing_formes_array) - 1
    
    if new_forme_pointer <= 0:
        print(f"\nCRITICAL ERROR: Calculated insertion index is {new_forme_pointer}.")
        print("This means the base Pokemon's pointer data is corrupted. Aborting to prevent data shift.")
        return poke_edit_data
    
    #repoint everyone after the inserted formes, just add new_forme_count to current pointer. 
    for index, pokemon in enumerate(poke_edit_data.personal):
        cur_forme_pointer = from_little_bytes_int(pokemon[0x1C:0x1E])
        #otherwise, if the current personal file's pointer is pointing to or after the newly inserted stuff, increment it by the number of new formes
        if(cur_forme_pointer >= new_forme_pointer):
            pokemon[0x1C:0x1E] = from_int_little_bytes(cur_forme_pointer + new_forme_count, 0x2)

    for x in range(new_forme_count):
        # Use list() to ensure deep copies that the compiler can concatenate
        poke_edit_data.personal.insert(new_forme_pointer + x, list(poke_edit_data.personal[personal_source_index]))
        poke_edit_data.levelup.insert(new_forme_pointer + x, list(poke_edit_data.levelup[levelup_source_index]))
        poke_edit_data.evolution.insert(new_forme_pointer + x, list(poke_edit_data.evolution[evolution_source_index]))
        existing_formes_array.append(new_forme_pointer + x)
    #finally, update all the existing files for this pokemon with total forme count and pointer
    #existing_formes_array now has all entries for this species

    #first determine what the pointer is - if there were already formes, keep using the old pointer
    use_pointer = 0
    if(old_forme_pointer != 0):
        use_pointer = old_forme_pointer
    else:
        use_pointer = new_forme_pointer

    for index in existing_formes_array:
        poke_edit_data.personal[index][0x20] = total_formes
        poke_edit_data.personal[index][0x1C:0x1E] = from_int_little_bytes(use_pointer, 0x2)

    #needed for the CSV update and stuff
    start_location = new_forme_pointer

    if(skip_model_insertion):
        poke_edit_data = update_csv_after_changes(poke_edit_data, base_form_index, new_forme_count, start_location, True)
    else:
        print("Copying new model data")
    
        #create new sets of model files
        #don't forget model index starts from 0 unlike everything else
        #first figure out which files we're copying
        total_previous_models = 0
        

        #first need to go to the four bytes for that *species*, which start at (index - 1)*4
        #the fourth byte, (index - 1)*4 + 3, is 0x1 when no formes, 0x3 when only alternate-gender formes, 0x7 when both
        #the third byte is how many models that species has.
        #The first and second bytes are a 2-byte little endian that is the *total number of models up to but not including the first model for that species*. So Bulby is 00 00 01 01 (no models before it), Venusaur is 02 00 03 07 (2 models before it, 3 models for Venusaur (male, female, and Mega), and has both gendered and non-gendered models
            
        #number of models on pokemon getting new forms + total number of models of all prior Pokemon
        total_previous_models = poke_edit_data.model_header[4*(base_form_index - 1) + 2] + from_little_bytes_int(poke_edit_data.model_header[4*(base_form_index - 1):4*(base_form_index - 1) + 2])
            
        #if going with default model, need to grab the base-forme's model for model_source_index
        if(def_model):
            model_source_index = poke_edit_data.model_header[4*(base_form_index - 1) + 1]*256 + poke_edit_data.model_header[4*(base_form_index - 1) + 0]

        #this is the model we're copying
        model_start_file = 0
        #this is where we have to insert those copies after
        model_dest_file = 0
        #this is how many of those files there are per new forme
        model_file_count = 0
        

        #exclude model header since that's not in my model file list structure (otherwise start file needs 1 added to it)
        #destination will be done via insert
        if(poke_edit_data.game == 'XY'):
            model_start_file = 8*(model_source_index)+3
            model_dest_file = 8*total_previous_models+3
            model_file_count = 8
        elif(poke_edit_data.game == "ORAS"):
            #Need someone to check
            model_start_file = 8*(model_source_index)+2
            model_dest_file = 8*total_previous_models+2
            model_file_count = 8
        #assume SM or USUM
        else:
            model_start_file = 9*(model_source_index)
            model_dest_file = 9*total_previous_models
            model_file_count = 9
            

        # Copy the lists properly using bytearray() to maintain binary compatibility
        temp = []
        for x in range(model_file_count):
            temp.append(bytearray(poke_edit_data.model[model_start_file + x]))

        # Create fresh bytearray copies for EVERY new forme
        files_to_insert = []
        for _ in range(new_forme_count):
            for file_data in temp:
                files_to_insert.append(bytearray(file_data))
                
        poke_edit_data.model[model_dest_file:model_dest_file] = files_to_insert

            
        print("Updating model header")
            
        #Now need to update model header
        '''
            0x1 == Any model exists?
            0x2 == Gender-difference model/forme exists. It seems that if this is set AND the next bit is not set, then the 2nd model goes to the 2nd personal file (it assumes it's an alt-gender forme like Meowstic)
            0x4 == other cause of forme models. If this is set, then the 2nd model is assumed to be for the female of the base species, and the 3rd model goes to the 2nd personal file, etc.
        '''
        #set the non-gendered form bit to true if not so set
        if(poke_edit_data.model_header[4*(base_form_index - 1) + 3] < 0x05):
            poke_edit_data.model_header[4*(base_form_index - 1) + 3] += 0x04
            
            
        #update the number of models for the species
        poke_edit_data.model_header[4*(base_form_index - 1) + 2] += new_forme_count
            
            
        #update the "model count so far" value
        #poke_edit_data.max_species_index + 1 because egg is last at position poke_edit_data.max_species_index (0 is Bulba, poke_edit_data.max_species_index - 1 is the last species in nat dex)
        #because for loop, would need to subtract 1 (0,poke_edit_data.max_species_index - 1) normally, but +1 for egg
            
        #grab model count so far
        model_count = 0
            
        #update number of models
        #start from the beginning just in case something erred at another point
        for index in range(0, poke_edit_data.max_species_index):
            poke_edit_data.model_header[4*index:4*index + 2] = from_int_little_bytes(model_count, 2)
            model_count += poke_edit_data.model_header[4*index + 2]
            
            

        #after the first part of the file, the next part (starting immediately after the last four bytes of the above described section of the model table header that mark the Egg model) has 2 bytes per *model*, the values of which have some pattern but make about no sense at all. Perhaps legacy data? In any event the game needs there be those 2 bytes per model, so the following section shifts this table forward by the appropriate amount to insert 00 00 for each new model added (not adding them to the end just in case something somehow needs those in the expected spot).
        #This section starts at poke_edit_data.max_species_index*4
           
            

        ''':

        Most pokemon are 00 00 as Axyn observed in gen VI

        Totem Raticate, Totem Marowak, and Zygarde 10% with Power Construct are 07 01

        Furfrou Star Trim, Furfrou Diamond Trim, and the Hat Pikachu for Johto, Sinnoh, Unova, Kalos, and Alola are 00 01

        All the other totems except for Ribombee, Zygarde 50% with Power Construct, Medium and Large Pumpkaboo and Gourgeist are 07 00

        Furfrou Matron Trim & Furfrou Dandy Trim are 00 04

        Furfrou Kabuki Trim, Furfrou Pharaoh Trim, and all Minior Core Formes except for Red are 00 07

        All Female Gender Difference models (except for Frillish, Jellicent, Pyroar, & Meowstic), all Silvally and Arceus (except for Normal), the entire Flabebe line except for Red and Eternal Floette, all Vivillon except for Icy Snow, Genesect w/ drive, Deerling/Sawsbuck except Spring, non-Zen Darmanitan, & Blue Basculin are 01 00

        '''
            
            
        #first entry (Bulbasaur model) is at 4*(max_species_index + 1)
        start_of_byte_flag_table = 4*(poke_edit_data.max_species_index + 1)
                
                
        #get the source model flag
        model_source_flag_offset = 2*model_source_index + start_of_byte_flag_table

        model_source_flags = poke_edit_data.model_header[model_source_flag_offset:model_source_flag_offset + 2]
        
        if(model_source_flags != [0x0, 0x0]):
            print('Warning, the model you chose has bitflags of ', bytes(model_source_flags[0]), bytes(model_source_flags[1]),' this might result in undesired behavior.')

        #bitflags will go to 2*(total_previous_models) + 1 plus the offset

        target_bitflag_offset = 2*(total_previous_models) + start_of_byte_flag_table + 1

        for x in range(new_forme_count):
            poke_edit_data.model_header.insert(target_bitflag_offset,model_source_flags[1])
            poke_edit_data.model_header.insert(target_bitflag_offset, model_source_flags[0])
        print('Model header updated')
        
        #update csv with new model indices & also the model type table
        poke_edit_data = update_csv_after_changes(poke_edit_data, base_form_index, new_forme_count, start_location, False, model_source_flags)

    if not defer_save:
        print('Writing updated GARCs (Updated the Model GARC will take a few minutes)' + '\n')

        save_and_refresh_GARCs(poke_edit_data)

            
        try:
            poke_edit_data = write_CSV(poke_edit_data)
        except:
            print('Please close your Pokemon Names and Files CSV if it is open')
            poke_edit_data.csv_pokemon_list_path = asksaveasfile(title='Select Pokemon Names and Files CSV')
            write_CSV(poke_edit_data)
        print('Pokemon Names and Files CSV updated' + '\n')
        
    #poke_edit_data = load_names_from_CSV(poke_edit_data)
                         
    print('Insertion complete!\n')
    return(poke_edit_data)