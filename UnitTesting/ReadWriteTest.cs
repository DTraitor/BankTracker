namespace UnitTesting;

public class ReadWriteTest
{
    private const string TestFileName = "DatabaseTest.json";

    [TearDown]
    public void CleanUp()
    {
        File.Delete(TestFileName);
    }

    [Test]
    public void DatabaseTest()
    {
        FileStream stream = new FileStream(TestFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        Bank definition = new Bank()
        {
            Name = "TestName",
            Address = "TestAddress",
            Ownership = Bank.OwnershipType.Private,
            DepositConditions = new Dictionary<Bank.DepositType, int>()
            {
                {Bank.DepositType.Demand, 1},
                {Bank.DepositType.Term, 2},
                {Bank.DepositType.Saving, 3},
            },
        };
        stream.SetLength(0);

        DatabaseReadWriter<Bank> database = new (stream);
        Assert.That(database.BankList, Is.EqualTo(new List<Bank>()));

        database.BankList.Add(definition);
        database.Dispose();
        stream.Close();

        database = new DatabaseReadWriter<Bank>(TestFileName);
        Assert.That(database.BankList, Is.EqualTo(new List<Bank>{definition}));
        database.Dispose();
    }
}