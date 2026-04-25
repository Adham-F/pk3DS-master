from utilities import *
from my_constants import *
    
#updated target Personal file with new Forme Count and First Forme Pointer
def personal_file_update(poke_edit_data, target_index, total_formes, start_location):
    #open target personal file

    #print('about to write ', new_forme_count)
            
    #set these two edits to only write if they are bigger than 0, allows this function to be called and only update one of them
    if(total_formes > 0):
        poke_edit_data.personal[target_index][0x20] = total_formes
    if(start_location > 0):
        poke_edit_data.personal[target_index][0x1C:0x1E] = from_int_little_bytes(start_location, 0x2)
    return(poke_edit_data)
            
'''#rebuilds personal compilation file
def concatenate_bin_files(folder_path):

    #generation = 7
    pad_count = 4
    #max_binary_file = 9999
    
    
    #grab list of filenames inside folder
    dir_list = os.listdir(folder_path)
    
    #check to see gen. Gen 6 has 3 char, 7 has 4, with extension is 7 and 8
    if(len(dir_list[0]) == 8):
        #generation = 7
        pad_count = 4
        #max_binary_file = 9999
    elif(len(dir_list[0]) == 7):
        #generation = 6
        pad_count = 3
        #max_binary_file = 999
    else:
        print("Error with filename")
        return
    #check to see if current directory has the compilation file:
    #grab last index
    
    #if length of last element is greater than dec 84, is compilation file (or something is wrong)
    if(os.path.getsize(os.path.join(folder_path, dir_list[-1])) > 84):
        # move the old compilation file to current directory and stick "backup_" in front of it
        dropbox_workaround_file_rename(os.path.join(folder_path, dir_list[-1]), 'backup_' + dir_list[-1])
        print('Backed up old compilation file to executable\'s directory')
        
        #remove compilation file from dir_list
        del dir_list[-1]
    
    #print(len(dir_list)-1, " Pokemon entries detected.")
    
    with open(os.path.join(folder_path, str(len(dir_list)).zfill(pad_count) + '.bin'), 'wb') as output_stream:
        for file_name in dir_list:
            file_path = os.path.join(folder_path, file_name)
            
            if os.path.exists(file_path):
                with open(file_path, 'rb') as file:
                    data = file.read()
                    output_stream.write(data)
    print('New compilation file created')
    
    return'''

def update_csv_after_changes(poke_edit_data, base_form_index, new_forme_count, start_location, inserted_bool = False,
                             model_source_flags=None):
    

    #get all row numbers of this species
    if model_source_flags is None:
        model_source_flags = [0x0, 0x0]
    working_indices = find_rows_with_column_matching(poke_edit_data.master_list_csv, 2, int(base_form_index))
    
    
    #grab the base species name since we'll be using that at least once
    base_species_name = poke_edit_data.master_list_csv[working_indices[0]][0]
    
    if(inserted_bool):
        #find the first row for this base species that doesn't have unique personal data. That is where this will be entered.
        first_missing_personal_line = 0
        for indices in working_indices:
            if(poke_edit_data.master_list_csv[indices][3] == ''):
                first_missing_personal_line = indices
                break
        if(first_missing_personal_line == 0):
            print('Error, no matching Pokemon found somehow')
        
        #update with temporary personal index number so the sorter can handle it
        
        first_free_index = max_of_column(poke_edit_data.master_list_csv, 3) + 1

        for offset in range(new_forme_count):
            poke_edit_data.master_list_csv[first_missing_personal_line + offset][3] = first_free_index + offset
    else:
        #index we start inserting the new rows from
        csv_insertion_point = max(working_indices) + 1


        #Second, we insert the new rows
        #base species index, personal file index, model index, species name, forme name
        for offset in range(new_forme_count):
            #note that model index is set to zero, since we will do one big sweep after this to update all that come after, anyway
            #Forme name is set to the number alt forme it is (e.g. if we add a forme to a Pokemon with 3 existing alt formes, it will be 4 (as the base species itself is 0))
            poke_edit_data.master_list_csv.insert(csv_insertion_point + offset, [base_species_name, len(working_indices) + offset, base_form_index, start_location + offset, 0, model_source_flags[0], model_source_flags[1]])
            

        #modelless_skip_count = 0
        #third we sweep through the entire array and update the model and personal index numbers, starting from the first newly inserted row
        for offset in range(1, len(poke_edit_data.master_list_csv)):
            
            #entry X has model index X, starting from 1. Last model is the Egg
            poke_edit_data.master_list_csv[offset][4] = offset
            
            try:
                #old personal index is at least the place we started inserting from and it's from a higher base index number (e.g. inserted new squirtle forme, all else unchanged first pass will be Mega Blastoise)
                if(poke_edit_data.master_list_csv[offset][3] >= start_location and poke_edit_data.master_list_csv[offset][2] > base_form_index):
                    poke_edit_data.master_list_csv[offset][3] += new_forme_count
            #is model-only
            except:
                pass
            
    #now redo the forme index numbers
    absolute_number = poke_edit_data.max_species_index + 1
    for entry in poke_edit_data.master_list_csv:
        try:
            if(entry[3] > poke_edit_data.max_species_index):
                entry[3] = absolute_number
                absolute_number += 1
        except:
            pass


    return(poke_edit_data)

def update_model_list(poke_edit_data):
    
    #if we haven't loaded the Personal file yet, we will need to do all the rest later
    if(len(poke_edit_data.personal) == 0):
        poke_edit_data.run_model_later = True
        print('Will initialize Model list after the Personal list')
        return
    else:
        poke_edit_data.run_model_later = False
        print('Initializing default Model list')

    model_temp_list = []
            
    #for base formes plus egg at poke_edit_data.max_species_index
    for index in range(poke_edit_data.max_species_index):
        #number of model sets this species has
        number_of_models = int(poke_edit_data.model_header[(index)*4 + 2])
              
        #grab the name

        if(index + 1 == poke_edit_data.max_species_index):
            temp_name = poke_edit_data.base_species_list[0]
        else:
            temp_name = poke_edit_data.base_species_list[index+1]
                
        #name each model as <Pokemon species> <number>, the base forme is named <Pokemon> 0 so that (in at least most cases) the model-number lines up with the forme-number (where there are multiple)
        for distinct_models in range(0, number_of_models):
            model_temp_list.append(temp_name + ' ' + str(distinct_models))
    
    #append the Egg
    model_temp_list.append('Egg')

    #copy this into the current list to initialize properly (particularly when loading from cfg)
    poke_edit_data.model_source_list = model_temp_list.copy()                
    poke_edit_data.current_model_source_list = poke_edit_data.model_source_list.copy()

    return(poke_edit_data)

'''def conditional_update_modelless_formes_list(poke_edit_data):
    if(poke_edit_data.modelless_formes == []):
        match poke_edit_data.game:
            case "XY":
                poke_edit_data.modelless_formes = xy_modelless_formes
            case "ORAS":
                poke_edit_data.modelless_formes = oras_modelless_formes
            case "SM":
                poke_edit_data.modelless_formes = sm_modelless_formes
            case "USUM":
                poke_edit_data.modelless_formes = usum_modelless_formes
    return(poke_edit_data)'''


def update_species_list(poke_edit_data, overwrite_from_default = False):
    
    if(overwrite_from_default or (len(poke_edit_data.master_list_csv) < 100)):
        print('Initializing default Species list')
        #set base species list based on which game we're dealing with
        #grab the species name from the master list
        if(poke_edit_data.game == "USUM"):
            poke_edit_data.master_list_csv = usum_master_list_csv.copy()
            poke_edit_data.base_species_list = entire_of_column(usum_master_list_csv, 0, False)
        if(poke_edit_data.game == "SM"):
            poke_edit_data.master_list_csv = sm_master_list_csv.copy()
            poke_edit_data.base_species_list = entire_of_column(sm_master_list_csv, 0, False)
        elif(poke_edit_data.game == "XY" or poke_edit_data.game == "ORAS"):
            poke_edit_data.master_list_csv = xy_master_list_csv.copy()
            poke_edit_data.base_species_list = entire_of_column(xy_master_list_csv, 0, False)
    else:
        poke_edit_data.base_species_list = entire_of_column(poke_edit_data.master_list_csv, 0, False)
        
    #remove the model-only Egg from the end:
    poke_edit_data.base_species_list.pop() 

    #first part of master formes list is just the base species list
    poke_edit_data.master_formes_list = poke_edit_data.base_species_list.copy()

    personal_index_count = len(poke_edit_data.personal)


    print("Initializing default Formes list")
    #adds (total number of pokemon personal files) - (total number of base species) spots to the end of the array
    for x in range(0, personal_index_count - poke_edit_data.max_species_index - 1):
        poke_edit_data.master_formes_list.append('')
        

    #print(poke_edit_data.max_species_index)
    #open personal
    #open each file until max species, note pointer and forme count
    #reference hardcoded list to spit out <species> <number> like pk3ds
    #iterate through the personal files
    for index, file in enumerate(poke_edit_data.personal):
        #since we're filling up the poke_edit_data.master_formes_list by iterating through the base formes, we can stop when we finish the last base Pokemon
        #poke_edit_data.max_species_index is here the first alt forme because off-by-1, so stop here

        #skip the first empty one

        if(index == 0):
            pass
        elif(index == poke_edit_data.max_species_index + 1):
            break
        #pull # of formes
        forme_count = file[0x20]
        try:
            forme_pointer = from_little_bytes_int(file[0x1C:0x1E])
        except Exception as e:
            print('Possible error detected', e)
        #if more than 1 AND forme pointer not 0, need to update those names in the array
        if(forme_count > 1 and forme_pointer != 0):
            #first forme in forme count is the base, need to do 1 less than that. 
            for x in range(0, forme_count - 1):
                target_index = forme_pointer + x
                
                # Dynamically expand the master formes list to prevent out-of-range errors
                while len(poke_edit_data.master_formes_list) <= target_index:
                    poke_edit_data.master_formes_list.append('')
                    
                poke_edit_data.master_formes_list[target_index] = poke_edit_data.base_species_list[index] + ' ' + str(x+1)
    #print(poke_edit_data.master_formes_list)
    #if we loaded Model before Personal, need to load Model now
    if(poke_edit_data.run_model_later):
        poke_edit_data = update_model_list(poke_edit_data)
        
    
    return(poke_edit_data)