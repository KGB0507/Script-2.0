import pandas as pd
import time

start_time = time.time()

class cryst_coord:
    def __init__(self, x, y, f1f2f3):
        self.x = x
        self.y = y
        self.f1f2f3 = f1f2f3

x = 0
dx = 650
y = 0
dy = 470

# Чтение файла Excel
df = pd.read_excel('Карта перемычек на пластине +цвета_НОРМ.xlsx')

# Номер строки, с которой нужно начать чтение (нулевая координата по x)
start_row = df[df['Координаты (мкм)'] == 0].index.item()

# Замена пустых значений на символ '-'
df.fillna('-', inplace=True)

# Номер стартового столбца (нулевая координата по y)
start_column = df.columns.get_loc(0)
current_column = start_column

# Номер последнего столбца, до которого нужно читать данные
# end_column = start_column + 1

# Список для хранения прочитанных столбцов
# data = []
# список для хранения перемычек каждого кристалла
cryst_coords = []

# Цикл по столбцам
while (df.iloc[start_row, current_column] != 0):
    column_data = df.iloc[:(start_row + 1), current_column].tolist()
    while 0 in column_data:
        column_data.remove(0)
    #data.append(column_data)
    for item in reversed(column_data):
        if item == '':
            item = '-'
        temp_cryst_coord = cryst_coord (x, y, item)
        y = y + dy
        cryst_coords.append(temp_cryst_coord)
    current_column = current_column + 1
    y = 0
    x = x + dx

# Вывод прочитанных данных
# for i, column_data in enumerate(data):
    #print(f"Столбец {i + 1}: {column_data}")

# Открытие файла для записи
with open('output.txt', 'w') as cryst_coordinates:
    for item in cryst_coords:
        cryst_coordinates.write(str(item.x) + '\t' + str(item.y) + '\t' + item.f1f2f3 + '\n')

# Load the xlsx file
# excel_data = pd.read_excel('Карта перемычек на пластине +цвета_НОРМ.xlsx')
# Read the values of the file in the dataframe
# column = pd.DataFrame(excel_data, columns=["Координаты (мкм)"])

# print("The content of the file is:\n", column)
# print("The content of the file is:\n", excel_data)pip
# column[column['Координаты (мкм)']] == '0'.index [0]

end_time = time.time()
elapsed_time = end_time - start_time
print('Elapsed time: ', elapsed_time)