type_chart = [1,1,1,1,1,0.5,1,0,0.5,1,1,1,1,1,1,1,1,1,2,1,0.5,0.5,1,2,0.5,0,2,1,1,1,1,0.5,2,1,2,0.5,1,2,1,1,1,0.5,2,1,0.5,1,1,2,0.5,1,1,1,1,1,1,1,1,0.5,0.5,0.5,1,0.5,0,1,1,2,1,1,1,1,1,2,1,1,0,2,1,2,0.5,1,2,2,1,0.5,2,1,1,1,1,1,1,0.5,2,1,0.5,1,2,1,0.5,2,1,1,1,1,2,1,1,1,1,0.5,0.5,0.5,1,1,1,0.5,0.5,0.5,1,2,1,2,1,1,2,0.5,0,1,1,1,1,1,1,2,1,1,1,1,1,2,1,1,0.5,1,1,1,1,1,1,2,1,1,0.5,0.5,0.5,1,0.5,1,2,1,1,2,1,1,1,1,1,0.5,2,1,2,0.5,0.5,2,1,1,2,0.5,1,1,1,1,1,1,2,2,1,1,1,2,0.5,0.5,1,1,1,0.5,1,1,1,1,0.5,0.5,2,2,0.5,1,0.5,0.5,2,0.5,1,1,1,0.5,1,1,1,1,2,1,0,1,1,1,1,1,2,0.5,0.5,1,1,0.5,1,1,1,2,1,2,1,1,1,1,0.5,1,1,1,1,0.5,1,1,0,1,1,1,2,1,2,1,1,1,0.5,0.5,0.5,2,1,1,0.5,2,1,1,1,1,1,1,1,1,1,1,0.5,1,1,1,1,1,1,2,1,0,1,0.5,1,1,1,1,1,2,1,1,1,1,1,2,1,1,0.5,0.5,1,2,1,0.5,1,1,1,1,0.5,0.5,1,1,1,1,1,2,2,1]


def main():
    output_1 = []
    output_2 = []
    iter_offset = 0
    #iterate over all combos of up to 2 types
    
    for type_1 in range(18):
        for type_2 in range(type_1, 18):
            #iterate over being attacked by each of the 18 types
            
            temp_atk_max = 0
            temp_def_min = 8
            temp_def_inf = 8
            
            temp_atk_arr = []
            
            for attack_type in range(18):
                attack_mult = type_chart[18*attack_type + type_1] * type_chart[18*attack_type + type_2]
                
                if(attack_mult >= temp_atk_max):
                    if(attack_mult > temp_atk_max):
                        temp_atk_arr = []
                        temp_atk_max = attack_mult
                
                    #if just found a better thing, temp_atk_arr = [], just set def_mult_min
                    def_mult_min = min(type_chart[attack_type + 18*type_1], type_chart[attack_type + 18*type_2])
                    if(def_mult_min <= temp_def_min or temp_atk_arr == []):
                        
                        def_mult_inf = max(type_chart[attack_type + 18*type_1], type_chart[attack_type + 18*type_2])
                        
                        if(def_mult_min < temp_def_min or temp_atk_arr == []):
                            temp_atk_arr = []
                            temp_def_min = def_mult_min
                            temp_def_inf = def_mult_inf
                        #if def_mult_min == temp_def_min, we need to check if it's better for the worst-case
                        elif(def_mult_inf <= temp_def_inf):
                            if(def_mult_inf < temp_def_inf):
                                temp_atk_arr = []
                                temp_def_inf = def_mult_inf
                    #now append attack_type. if we found a better one, it is being appended to an empty array, otherwise it is building bigger
                    temp_atk_arr.append(attack_type)
            temp_atk_arr.insert(0, len(temp_atk_arr))
            iter_offset += len(temp_atk_arr)
            output_1.append(iter_offset)
            #output_2.append(temp_atk_arr)
    print(output_1)
    print('/n/n/n/n')
    print(output_2)
                    

main()