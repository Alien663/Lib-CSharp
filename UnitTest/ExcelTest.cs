using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Alien.Common.Excel;
using Newtonsoft.Json;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace TestMyLib;

[TestFixture, Order(1)]
public class Write_DataTable
{
    private DataTable dtData = GetInitialData.GetDataTable();

    [Test]
    public void DataTable2Excel()
    {
        #region Arrange
        string filename = @".\DataTable.xlsx";
        #endregion

        #region Act
        using ExcelWriter writer = new ExcelWriter();
        byte[] data = writer.Export(dtData);
        using (FileStream fs = File.Create(filename))
        {
            fs.Write(data, 0, data.Length);
        }
        #endregion

        #region Assert
        FileAssert.Exists(filename);
        byte[] myfile = File.ReadAllBytes(filename);
        using (ExcelReader reader = new ExcelReader())
        {
            DataTable result = reader.ToDataTable(myfile);
            for (int i = 0; i < result.Rows.Count; i++)
            {
                for (int j = 0; j < result.Columns.Count; j++)
                {
                    Assert.That(result.Rows[i][j].ToString(), Is.EqualTo(dtData.Rows[i][j].ToString()));
                }
            }
        }
        #endregion
    }

    [Test]
    public void DataTable2Excel_Anchor()
    {
        #region Arrange
        string filename = @".\DataTable_Anchor.xlsx";
        #endregion

        #region Act
        using ExcelWriter writer = new ExcelWriter();
        writer.SetAnchor(2, 3);
        byte[] data = writer.Export(dtData);
        using (FileStream fs = File.Create(filename))
        {
            fs.Write(data, 0, data.Length);
        }
        #endregion

        #region Assert
        FileAssert.Exists(filename);
        byte[] myfile = File.ReadAllBytes(filename);
        using (ExcelReader reader = new ExcelReader())
        {
            reader.SetAnchor(2, 3);
            DataTable result = reader.ToDataTable(myfile);
            for (int i = 0; i < result.Rows.Count; i++)
            {
                for (int j = 0; j < result.Columns.Count; j++)
                {
                    Assert.That(result.Rows[i][j].ToString(), Is.EqualTo(dtData.Rows[i][j].ToString()));
                }
            }
        }
        #endregion
    }

    [Test]
    public void DataTable2Excel_DataType()
    {
        #region Arrange
        string filename = @".\DataTable_DataType.xlsx";
        #endregion

        #region Act
        using ExcelWriter writer = new ExcelWriter();
        writer.setDataTypeStyle(new Dictionary<string, string> { { "Double", "#,##0.0000" } });
        byte[] data = writer.Export(dtData);
        using (FileStream fs = File.Create(filename))
        {
            fs.Write(data, 0, data.Length);
        }
        #endregion

        #region Assert
        FileAssert.Exists(filename);
        byte[] myfile = File.ReadAllBytes(filename);
        using (ExcelReader reader = new ExcelReader())
        {
            DataTable result = reader.ToDataTable(myfile);
            for (int i = 0; i < result.Rows.Count; i++)
            {
                for (int j = 0; j < result.Columns.Count; j++)
                {
                    Assert.That(result.Rows[i][j].ToString(), Is.EqualTo(dtData.Rows[i][j].ToString()));
                }
            }
        }
        #endregion
    }

    [OneTimeTearDown]
    public void CleanFile()
    {
        File.Delete(@".\DataTable.xlsx");
        File.Delete(@".\DataTable_Anchor.xlsx");
        File.Delete(@".\DataTable_DataType.xlsx");
    }
}

[TestFixture, Order(2)]
public class Write_DataSet
{
    private DataSet dsData = GetInitialData.GetDataSet();

    [Test]
    public void DataSet2Excel()
    {
        #region Arrange
        string filename = @".\DataSet.xlsx";
        #endregion

        #region Act
        using ExcelWriter writer = new ExcelWriter();
        byte[] data = writer.Export(dsData);
        using (FileStream fs = File.Create(filename))
        {
            fs.Write(data, 0, data.Length);
        }
        #endregion

        #region Assert
        FileAssert.Exists(filename);
        byte[] myfile = File.ReadAllBytes(filename);
        using (ExcelReader reader = new ExcelReader())
        {
            DataSet result = reader.ToDataSet(myfile);
            for (int k = 0; k < result.Tables.Count; k++)
            {
                for (int i = 0; i < result.Tables[k].Rows.Count; i++)
                {
                    for (int j = 0; j < result.Tables[k].Columns.Count; j++)
                    {
                        Assert.That(result.Tables[k].Rows[i][j].ToString(), Is.EqualTo(dsData.Tables[k].Rows[i][j].ToString()));
                    }
                }
            }
        }
        #endregion
    }

    [Test]
    public void DataSet2Excel_Anchor()
    {
        #region Arrange
        string filename = @".\DataSet_Anchor.xlsx";
        #endregion

        #region Act
        using ExcelWriter writer = new ExcelWriter();
        writer.SetAnchor(2, 3);
        byte[] data = writer.Export(dsData);
        using (FileStream fs = File.Create(filename))
        {
            fs.Write(data, 0, data.Length);
        }
        #endregion

        #region Assert
        FileAssert.Exists(filename);
        byte[] myfile = File.ReadAllBytes(filename);
        using (ExcelReader reader = new ExcelReader())
        {
            reader.SetAnchor(2, 3);
            DataSet result = reader.ToDataSet(myfile);
            for (int k = 0; k < result.Tables.Count; k++)
            {
                for (int i = 0; i < result.Tables[k].Rows.Count; i++)
                {
                    for (int j = 0; j < result.Tables[k].Columns.Count; j++)
                    {
                        Assert.That(result.Tables[k].Rows[i][j].ToString(), Is.EqualTo(dsData.Tables[k].Rows[i][j].ToString()));
                    }
                }
            }
        }
        #endregion
    }

    [Test]
    public void DataSet2Excel_SheetRange()
    {
        #region Arrange
        string filename = @".\DataSet_SheetRange.xlsx";
        #endregion

        #region Act
        using ExcelWriter writer = new ExcelWriter();
        writer.SetSheetRange(0, 1);
        byte[] data = writer.Export(dsData);
        using (FileStream fs = File.Create(filename))
        {
            fs.Write(data, 0, data.Length);
        }
        #endregion

        #region Assert
        FileAssert.Exists(filename);
        byte[] myfile = File.ReadAllBytes(filename);
        using (ExcelReader reader = new ExcelReader())
        {
            reader.SetSheetRange(0, 1);
            DataSet result = reader.ToDataSet(myfile);
            for (int k = 0; k < result.Tables.Count; k++)
            {
                for (int i = 0; i < result.Tables[k].Rows.Count; i++)
                {
                    for (int j = 0; j < result.Tables[k].Columns.Count; j++)
                    {
                        Assert.That(result.Tables[k].Rows[i][j].ToString(), Is.EqualTo(dsData.Tables[k].Rows[i][j].ToString()));
                    }
                }
            }
        }
        #endregion
    }

    [Test]
    public void DataSet2Excel_DataType()
    {
        #region Arrange
        string filename = @".\DataSet_DataType.xlsx";
        #endregion

        #region Act
        using ExcelWriter writer = new ExcelWriter();
        writer.setDataTypeStyle(new Dictionary<string, string> { { "Double", "#,##0.0000" } });
        byte[] data = writer.Export(dsData);
        using (FileStream fs = File.Create(filename))
        {
            fs.Write(data, 0, data.Length);
        }
        #endregion

        #region Assert
        FileAssert.Exists(filename);
        byte[] myfile = File.ReadAllBytes(filename);
        using (ExcelReader reader = new ExcelReader())
        {
            DataSet result = reader.ToDataSet(myfile);
            for (int k = 0; k < result.Tables.Count; k++)
            {
                for (int i = 0; i < result.Tables[k].Rows.Count; i++)
                {
                    for (int j = 0; j < result.Tables[k].Columns.Count; j++)
                    {
                        Assert.That(result.Tables[k].Rows[i][j].ToString(), Is.EqualTo(dsData.Tables[k].Rows[i][j].ToString()));
                    }
                }
            }
        }
        #endregion
    }

    [OneTimeTearDown]
    public void CleanFile()
    {
        File.Delete(@".\DataSet.xlsx");
        File.Delete(@".\DataSet_Anchor.xlsx");
        File.Delete(@".\DataSet_SheetRange.xlsx");
        File.Delete(@".\DataSet_DataType.xlsx");
    }
}

[TestFixture, Order(3)]
public class Write_DataModel
{
    private List<StudentModel> studentModels = GetInitialData.GetDataModel();

    [Test]
    public void DataModel2Excel()
    {
        #region Arrange
        string filename = @".\DataModel.xlsx";
        #endregion

        #region Act
        using ExcelWriter writer = new ExcelWriter();
        byte[] data = writer.Export(studentModels);
        using (FileStream fs = File.Create(filename))
        {
            fs.Write(data, 0, data.Length);
        }
        #endregion

        #region Assert
        FileAssert.Exists(filename);
        byte[] myfile = File.ReadAllBytes(filename);
        using (ExcelReader reader = new ExcelReader())
        {
            List<StudentModel> result = reader.ToList<StudentModel>(myfile);
            for (int i = 0; i < result.Count; i++)
            {
                Assert.That(result[i].StudentId, Is.EqualTo(studentModels[i].StudentId));
                Assert.That(result[i].Name, Is.EqualTo(studentModels[i].Name));
                Assert.That(result[i].Age, Is.EqualTo(studentModels[i].Age));
            }
        }
        #endregion
    }

    [Test]
    public void DataModel2Excel_Anchor()
    {
        #region Arrange
        string filename = @".\DataModel_Anchor.xlsx";
        #endregion

        #region Act
        using ExcelWriter writer = new ExcelWriter();
        writer.SetAnchor(2, 3);
        byte[] data = writer.Export(studentModels);
        using (FileStream fs = File.Create(filename))
        {
            fs.Write(data, 0, data.Length);
        }
        #endregion

        #region Assert
        FileAssert.Exists(filename);
        byte[] myfile = File.ReadAllBytes(filename);
        using (ExcelReader reader = new ExcelReader())
        {
            reader.SetAnchor(2, 3);
            List<StudentModel> result = reader.ToList<StudentModel>(myfile);
            for (int i = 0; i < result.Count; i++)
            {
                Assert.That(result[i].StudentId, Is.EqualTo(studentModels[i].StudentId));
                Assert.That(result[i].Name, Is.EqualTo(studentModels[i].Name));
                Assert.That(result[i].Age, Is.EqualTo(studentModels[i].Age));
            }
        }
        #endregion
    }

    [Test]
    public void DataModel2Excel_DataType()
    {
        #region Arrange
        string filename = @".\DataModel_DataType.xlsx";
        #endregion

        #region Act
        using ExcelWriter writer = new ExcelWriter();
        writer.setDataTypeStyle(new Dictionary<string, string> { { "Double", "#,##0.0000" } });
        byte[] data = writer.Export(studentModels);
        using (FileStream fs = File.Create(filename))
        {
            fs.Write(data, 0, data.Length);
        }
        #endregion

        #region Assert
        FileAssert.Exists(filename);
        byte[] myfile = File.ReadAllBytes(filename);
        using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
        {
            MemoryStream ms = new MemoryStream();
            fs.CopyTo(ms);
            ms.Position = 0;
            using (ExcelReader reader = new ExcelReader())
            {
                List<StudentModel> result = reader.ToList<StudentModel>(myfile);
                for (int i = 0; i < result.Count; i++)
                {
                    Assert.That(result[i].StudentId, Is.EqualTo(studentModels[i].StudentId));
                    Assert.That(result[i].Name, Is.EqualTo(studentModels[i].Name));
                    Assert.That(result[i].Age, Is.EqualTo(studentModels[i].Age));
                }
            }
        }
        #endregion
    }

    [OneTimeTearDown]
    public void CleanFile()
    {
        File.Delete(@".\DataModel.xlsx");
        File.Delete(@".\DataModel_Anchor.xlsx");
        File.Delete(@".\DataModel_DataType.xlsx");
    }
}

public static class GetInitialData
{
    private static DataTable dtData = new DataTable();
    private static DataSet dsData = new DataSet();
    public static DataTable GetDataTable()
    {
        dtData.TableName = "Test 1";
        dtData.Columns.Add("StudentId", typeof(int));
        dtData.Columns.Add("Name", typeof(string));
        dtData.Columns.Add("Age", typeof(double));
        dtData.Rows.Add(10000, "Jack", 15.00);
        dtData.Rows.Add(10100, "Smith", 17.02);
        dtData.Rows.Add(10200, "Keroro", 20.321);
        return dtData;
    }

    public static DataSet GetDataSet()
    {
        DataTable dtData1 = new DataTable();
        dtData1.TableName = "Test 1";
        dtData1.Columns.Add("StudentId", typeof(int));
        dtData1.Columns.Add("Name", typeof(string));
        dtData1.Columns.Add("Age", typeof(double));
        dtData1.Rows.Add(10000, "Jack", 15.00);
        dtData1.Rows.Add(10100, "Smith", 17.02);
        dtData1.Rows.Add(10200, "Keroro", 20.321);
        dsData.Tables.Add(dtData1);

        DataTable dtData2 = new DataTable();
        dtData2.TableName = "Test 2";
        dtData2.Columns.Add("StudentId", typeof(int));
        dtData2.Columns.Add("Name", typeof(string));
        dtData2.Columns.Add("Age", typeof(double));
        dtData2.Rows.Add(10300, "Rose", 14.00);
        dtData2.Rows.Add(10400, "Ted", 16.01);
        dtData2.Rows.Add(10500, "Tamama", 21.123);
        dsData.Tables.Add(dtData2);
        return dsData;
    }

    public static List<StudentModel> GetDataModel()
    {
        return new List<StudentModel>
        {
            new StudentModel {Name = "Jack", Age = 15, StudentId = 100},
            new StudentModel {Name = "Smith", Age = 17, StudentId = 101 },
            new StudentModel {Name = "Keroro", Age = 20, StudentId = 102 },
        };
    }
}
