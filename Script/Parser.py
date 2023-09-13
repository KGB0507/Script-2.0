import pandas as pd
import time

start_time = time.time()

class cryst_coord:
    def __init__(self, x, y, jumpers):
        self.x = x
        self.y = y
        self.jumpers = jumpers

def is_cryst(num_of_column):
    row = 0
    while(df.iloc[row, num_of_column] == 0):
        row += 1
    if(df.iloc[row, num_of_column] != 0):
        return True
    return False

x = 0
dx = 650
y = 0
dy = 470
num_of_crystals = 0

# Чтение файла Excel
df = pd.read_excel('Карта перемычек на пластине +цвета_НОРМ.xlsx')

# Замена пустых значений на символ '-'
df.fillna('-', inplace=True)

# Номер строки, с нулевой координатой по x
nul_row = df[df['Координаты (мкм)'] == 0].index.item()

# Номер столбца с нулевой координатой по y
nul_column = df.columns.get_loc(0)

# Номер стартового столбца
current_column = 1

# список для хранения перемычек каждого кристалла
cryst_coords = []

# Цикл по столбцам
try:
    while (current_column < len(df.columns)):
        # Поиск строки, в которой находится самый нижний кристалл в стартовом столбце
        #print('while 1')
        row = 0
        while (df.iloc[row, current_column] == 0):
            row += 1
        last_row = row

        #print(last_row + 2)
        #input()

        #print('while 2')
        while(df.iloc[row, current_column] != 0 and row < len(df.index) - 1 ):
            row += 1
        first_row = row - 1
        #print(first_row + 2)
        #input()

        column_data = df.iloc[(last_row - 1):(first_row + 1), current_column].tolist()
        while 0 in column_data:
            column_data.remove(0)
        row = first_row
        x = (current_column - nul_column) * dx
        for item in reversed(column_data):
            y = (nul_row - row) * dy
            temp_cryst_coord = cryst_coord (x, y, item)
            cryst_coords.append(temp_cryst_coord)
            num_of_crystals += 1
            row -= 1
        current_column += 1
        #print(f'end current_column {current_column-1}')
except:
    print(f'row - 1 = {row - 1}, current_column = {current_column}, {df.iloc[row-1, current_column]}, {len(df.index)}')

# Открытие файла для записи
with open('CrystCoordinates.txt', 'w') as cryst_coordinates:
    for item in cryst_coords:
        cryst_coordinates.write(str(item.x) + '\t' + str(item.y) + '\t' + item.jumpers + '\n')

print(f'Number of crystals: {num_of_crystals}')
print(f'NUMOFCOLS = {len(df.columns)}')
print(f'WDISTBETWCRYST + WIDTHOFCRYST = {dx}')

end_time = time.time()
elapsed_time = end_time - start_time
print('Elapsed time: ', elapsed_time)