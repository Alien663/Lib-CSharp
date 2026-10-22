using NUnit.Framework;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Alien.Common.Utility;

namespace TestMyLib;

[TestFixture]
public class UtilityTest
{
    [Test]
    public void DataTable2ClassModel()
    {
        #region Arrange
        var dt = new DataTable();
        dt.Columns.Add("StudentId", typeof(int));
        dt.Columns.Add("Name", typeof(string));
        dt.Columns.Add("Age", typeof(double));
        dt.Rows.Add(100, "John", 30);
        dt.Rows.Add(101, "Jane", 25);
        #endregion

        #region Action
        var result = dt.ToList<StudentModel>();
        #endregion

        #region Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].StudentId, Is.EqualTo(100));
        Assert.That(result[0].Name, Is.EqualTo("John"));
        Assert.That(result[0].Age, Is.EqualTo(30));
        Assert.That(result[1].StudentId, Is.EqualTo(101));
        Assert.That(result[1].Name, Is.EqualTo("Jane"));
        Assert.That(result[1].Age, Is.EqualTo(25));
        #endregion
    }

    [Test]
    public void ClassModel2DataTable()
    {
        #region Arrange
        List<StudentModel> students = new List<StudentModel>
        {
            new StudentModel{ StudentId = 100, Name = "John", Age = 30 },
            new StudentModel{ StudentId = 101, Name = "Jane", Age = 25 },
        };
        #endregion

        #region Act
        DataTable dt = ListExtensions.ToDataTable(students);
        #endregion

        #region Assert
        Assert.That(dt.Rows.Count, Is.EqualTo(2));
        Assert.That(dt.Rows[0]["StudentId"].ToString(), Is.EqualTo("100"));
        Assert.That(dt.Rows[0]["Name"].ToString(), Is.EqualTo("John"));
        Assert.That(dt.Rows[0]["Age"].ToString(), Is.EqualTo("30"));
        Assert.That(dt.Rows[1]["StudentId"].ToString(), Is.EqualTo("101"));
        Assert.That(dt.Rows[1]["Name"].ToString(), Is.EqualTo("Jane"));
        Assert.That(dt.Rows[1]["Age"].ToString(), Is.EqualTo("25"));
        #endregion
    }

    [Test]
    public void SegmentationTest()
    {
        #region Arrange
        string test = @"壬戌之秋，七月既望，蘇子與客泛舟遊於赤壁之下。清風徐來？水波不興；";
        #endregion

        #region Act
        List<TokenModel> result = ContextIndexing.Segment(test);
        #endregion

        #region Assert
        Assert.That(result.Count, Is.EqualTo(5));
        Assert.That(result[0].Context, Is.EqualTo("壬戌之秋"));
        Assert.That(result[0].Mark, Is.EqualTo("，"));
        Assert.That(result[1].Context, Is.EqualTo("七月既望"));
        Assert.That(result[1].Mark, Is.EqualTo("，"));
        Assert.That(result[2].Context, Is.EqualTo("蘇子與客泛舟遊於赤壁之下"));
        Assert.That(result[2].Mark, Is.EqualTo("。"));
        Assert.That(result[3].Context, Is.EqualTo("清風徐來"));
        Assert.That(result[3].Mark, Is.EqualTo("？"));
        Assert.That(result[4].Context, Is.EqualTo("水波不興"));
        Assert.That(result[4].Mark, Is.EqualTo("；"));
        #endregion
    }

    [Test]
    public void TokenizationTest()
    {
        #region Arrange
        string test = @"唧唧復唧唧";
        #endregion

        #region Act
        List<TokenModel> result = ContextIndexing.Tokenize(test, 4);
        #endregion

        #region Assert
        Assert.That(result.Count, Is.EqualTo(14));
        Assert.That(result[0].Context, Is.EqualTo("唧"));
        Assert.That(result[1].Context, Is.EqualTo("唧"));
        Assert.That(result[2].Context, Is.EqualTo("復"));
        Assert.That(result[3].Context, Is.EqualTo("唧"));
        Assert.That(result[4].Context, Is.EqualTo("唧"));
        Assert.That(result[5].Context, Is.EqualTo("唧唧"));
        Assert.That(result[6].Context, Is.EqualTo("唧復"));
        Assert.That(result[7].Context, Is.EqualTo("復唧"));
        Assert.That(result[8].Context, Is.EqualTo("唧唧"));
        Assert.That(result[9].Context, Is.EqualTo("唧唧復"));
        Assert.That(result[10].Context, Is.EqualTo("唧復唧"));
        Assert.That(result[11].Context, Is.EqualTo("復唧唧"));
        Assert.That(result[12].Context, Is.EqualTo("唧唧復唧"));
        Assert.That(result[13].Context, Is.EqualTo("唧復唧唧"));
        #endregion
    }
}
