namespace UnitTesting;

public class LogicTests
{
    private const string TestFileName = "BusinessLogicTest.json";

    [TearDown]
    public void CleanUp()
    {
        File.Delete(TestFileName);
    }

    [Test]
    public void BusinessLogicTests()
    {
        FileStream stream = new FileStream(TestFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        stream.SetLength(0);
        Logic interaction = new Logic(stream);

        Bank definition = interaction.CreateBank(
            "TestName",
            "TestAddress",
            Bank.OwnershipType.Private,
            new Dictionary<Bank.DepositType, int>()
            {
                {Bank.DepositType.Demand, 1},
                {Bank.DepositType.Term, 2},
                {Bank.DepositType.Saving, 3},
            }
            );
        Assert.That(interaction.GetBanks(), Is.EqualTo(new List<Bank>{definition}));
        interaction.ChangeName(definition, "TestName2");
        Assert.That(definition.Name, Is.EqualTo("TestName2"));
        interaction.ChangeAddress(definition, "TestAddress2");
        Assert.That(definition.Address, Is.EqualTo("TestAddress2"));
        interaction.ChangeOwnership(definition, Bank.OwnershipType.State);
        Assert.That(definition.Ownership, Is.EqualTo(Bank.OwnershipType.State));
        interaction.ChangeDepositCondition(definition, Bank.DepositType.Demand, 4);
        interaction.RemoveDepositCondition(definition, Bank.DepositType.Saving);
        Assert.That(definition.DepositConditions, Is.EqualTo(new Dictionary<Bank.DepositType, int>()
            {
                {Bank.DepositType.Demand, 4},
                {Bank.DepositType.Term, 2},
            }));

        interaction = new Logic(stream);
        Assert.That(interaction.GetBanks(), Is.EqualTo(new List<Bank>{definition}));
        Bank newDefinition = interaction.CreateBank(
            "TestName",
            "TestAddress",
            Bank.OwnershipType.Private,
            new Dictionary<Bank.DepositType, int>()
            {
                {Bank.DepositType.Demand, 1},
                {Bank.DepositType.Term, 2},
                {Bank.DepositType.Saving, 3},
            }
            );
        Assert.That(interaction.GetBanks(), Is.EqualTo(new List<Bank>{definition, newDefinition}));

        Assert.That(interaction.FindBestBank(Bank.DepositType.Demand, 1000, 2), Is.EqualTo(definition));

        interaction.DeleteBank(interaction.GetBanks()[0]);
        Assert.That(interaction.GetBanks(), Is.EqualTo(new List<Bank>{newDefinition}));

        stream.Close();

        interaction = new Logic(TestFileName);
        Assert.That(interaction.GetBanks(), Is.EqualTo(new List<Bank>{newDefinition}));
        interaction.Dispose();
    }
}