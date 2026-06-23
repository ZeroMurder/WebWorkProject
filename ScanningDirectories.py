import os
import sys

def print_tree(directory, prefix="", is_last=True):
    """
    Рекурсивно выводит содержимое директории в виде дерева.
    
    Args:
        directory (str): Путь к директории
        prefix (str): Префикс для форматирования (используется при рекурсии)
        is_last (bool): Является ли текущий элемент последним в родительской директории
    """
    # Проверяем существование директории
    if not os.path.exists(directory):
        print(f"Ошибка: Директория '{directory}' не существует")
        return
    
    if not os.path.isdir(directory):
        print(f"Ошибка: '{directory}' не является директорией")
        return
    
    # Получаем список содержимого директории
    try:
        items = os.listdir(directory)
    except PermissionError:
        print(f"{prefix}└── [Нет доступа]")
        return
    
    # Сортируем: сначала папки, затем файлы
    folders = []
    files = []
    for item in items:
        full_path = os.path.join(directory, item)
        if os.path.isdir(full_path):
            folders.append(item)
        else:
            files.append(item)
    
    # Сортируем алфавитно
    folders.sort()
    files.sort()
    
    # Объединяем: сначала папки, потом файлы
    sorted_items = folders + files
    
    # Выводим содержимое
    for i, item in enumerate(sorted_items):
        is_last_item = (i == len(sorted_items) - 1)
        full_path = os.path.join(directory, item)
        
        # Определяем символы для ветвления
        if is_last_item:
            branch = "└── "
            next_prefix = prefix + "    "
        else:
            branch = "├── "
            next_prefix = prefix + "│   "
        
        # Выводим элемент
        if os.path.isdir(full_path):
            print(f"{prefix}{branch}{item}/")
            # Рекурсивно обрабатываем поддиректорию
            print_tree(full_path, next_prefix, is_last_item)
        else:
            # Получаем размер файла
            try:
                size = os.path.getsize(full_path)
                size_str = format_size(size)
                print(f"{prefix}{branch}{item} ({size_str})")
            except:
                print(f"{prefix}{branch}{item}")

def format_size(bytes):
    """Форматирует размер в удобочитаемый вид"""
    for unit in ['Б', 'КБ', 'МБ', 'ГБ']:
        if bytes < 1024.0:
            return f"{bytes:.1f} {unit}"
        bytes /= 1024.0
    return f"{bytes:.1f} ТБ"

def main():
    # Получаем путь из аргументов командной строки или используем текущую директорию
    if len(sys.argv) > 1:
        directory_path = sys.argv[1]
    else:
        directory_path = os.getcwd()
    
    # Преобразуем в абсолютный путь
    directory_path = os.path.abspath(directory_path)
    
    print(f"Структура директории: {directory_path}\n")
    print(f"{os.path.basename(directory_path)}/")
    
    # Выводим дерево
    print_tree(directory_path, "", True)

if __name__ == "__main__":
    main()