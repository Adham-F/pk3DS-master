import os
import pathlib


#read input bytestring as little-endian, return integer
def from_little_bytes_int(byte_input):
    return int.from_bytes(byte_input, byteorder='little')

#convert integer input into little-endian hex with given padding (default 0x4 bytes)
def from_int_little_bytes(decimal_number, padding = 0x4):
    return(decimal_number.to_bytes(padding, 'little'))

#pull a given non-empty column from the given table and returns the max
def max_of_column(input_table, column_number) -> int:
    max_temp = 0
    for rows in input_table:
        try:
            if (int(rows[column_number]) > max_temp):
                max_temp = int(rows[column_number])
        except ValueError:
            max_temp = max_temp

    return(max_temp)

#pull non-empty entries from a given column from the given table and returns it
def entire_of_column(input_table, column_number, allow_multiple = True):
    table_temp = []
    last_element = ''
    for rows in input_table:
        if(not(rows[column_number] in {'', "NA"}) and (allow_multiple or rows[column_number] != last_element)):
            table_temp.append(rows[column_number])
        last_element = rows[column_number]
    return(table_temp)

#returns a list of the indices of the rows that contain the specified search term in the specified column. If only_one is true, then it returns the first one it finds instead
def find_rows_with_column_matching(input_table, column_number, search_term, only_one = False):
    found_table = []
    for row_index, rows in enumerate(input_table):
        if(rows[column_number] == search_term):
            if(only_one):
                return(row_index)
            else:
                found_table.append(row_index)
    return(found_table)

#streamline the file name-calling
def file_namer(folder, index, length, poke_edit_data, file_prefix = ''):
    
    return(os.path.join(folder, file_prefix + str(index).zfill(length)) + poke_edit_data.extracted_extension)

#removes file, exception thrown if not exist
def silentremove(filename: str) -> None:
    pathlib.Path(filename).unlink(missing_ok=True)
		
#convert integer to little endian as long as two bytes at most
def little_endian_chunks(big_input: int) -> tuple[int, int]:
    little = big_input.to_bytes(2, byteorder="little")
    return (little[0], little[1])


#returns list of specified columns from specified table
def entire_of_columns(input_table, column_numbers_list):
    table_temp = []
    for row_input in input_table:
        row_temp = []
        for column in column_numbers_list:
                row_temp.append(row_input[column])
        table_temp.append(row_temp)
    return(table_temp)


#takes the table and reorders it so that the thing going somewhere empty has room, then moves the thing to where that one was, etc. Then repeats until done
def sort_table_personal_files(to_sort_table):
            
    #old, new, pointer
    order_table = []
    

    #there's a big gap between the last "real" personal file and the newly insert files, need to find the last real one
    #find the smallest number in old that's bigger than the biggest in new. This is the first new forme file, so the real max personal pre-insert will be the next biggest number    
    max_personal_new = max_of_column(to_sort_table, 1)
    
    smallest_dummy_personal_old = 999999

    for row in to_sort_table:
        if(max_personal_new < row[0] < smallest_dummy_personal_old):
            smallest_dummy_personal_old = row[0]
            
    max_personal_old = 0 
    for row in to_sort_table:
        if(max_personal_old < row[0] < smallest_dummy_personal_old):
            max_personal_old = row[0]
                



   # try:
    while True:
        #finds the highest destination file number left, adds that
        order_table.append(to_sort_table.pop(find_rows_with_column_matching(to_sort_table, 1, max_of_column(to_sort_table, 1))[0]))
        
        
        #if these are equal, no chain to follow
        if(order_table[-1][0] != order_table[-1][1]):
            while True:
                #takes the most recently added line the order table, and gets the filenumber it came from.
                next_personal = order_table[-1][0]
    
                #if next_personal is bigger than the largest destination file (max_personal), the file we just moved is one of the newly inserted Pokemon, so there is nothing more to add from this chain,
                if(next_personal >= max_personal_old or len(to_sort_table) == 0):
                    break
                else:
                    #grab the thing that is going to where the previous file was
                    order_table.append(to_sort_table.pop(find_rows_with_column_matching(to_sort_table, 1, next_personal)[0]))
                    
            if(len(to_sort_table) == 0):
                    break
        elif(len(to_sort_table) == 0):
            break
    for x in to_sort_table:
        order_table.append(x)
        
    return(order_table)