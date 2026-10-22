# Alien.Common.Excel

A library for importing/exporting Excel files using NPOI.

## 📦 Installation

```bash
Install-Package Alien.Common.Excel
```

## 🚀 Features

- Export data to Excel from List\<T>/DataTable/DataSet
- Import data from Excel to List\<T>/DataTable/DataSet
- Support for multiple sheets in a single file when exporting by DataSet
- Support for setting the starting row for data
- Apply styling and formatting

## 🧪 Example Usage

```csharp
using Alien.Common.Excel;

var data = new List<MyModel>
{
    new MyModel { Name = "John", Age = 30 },
    new MyModel { Name = "Jane", Age = 25 }
};

using ExcelConverter excel = new ExcelConverter();
using FileStream fs = File.Create(filename);
byte[] data = excel.export(rawData);
fs.Write(data, 0, data.Length);
```

## 📘 Dependencies

- Closed XML

## 🛠 Compatibility

- .NET 10 and above

## 🧙 Author

Alien663
