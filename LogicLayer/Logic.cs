using DatabaseAccess;

namespace LogicLayer;

public class Logic : IDisposable
{
    public Logic(string fileName)
    {
        databaseAccess = new DatabaseReadWriter<Bank>(fileName);
    }

    public Logic(FileStream stream)
    {
        databaseAccess = new DatabaseReadWriter<Bank>(stream);
    }

    public void Dispose()
    {
        databaseAccess.Dispose();
    }
    public Bank CreateBank(string name, string address, Bank.OwnershipType ownership, Dictionary<Bank.DepositType, int> depositConditions)
    {
        Bank bank = new()
        {
            Name = name,
            Address = address,
            Ownership = ownership,
            DepositConditions = depositConditions
        };
        databaseAccess.BankList.Add(bank);
        databaseAccess.Save();
        return bank;
    }

    public void DeleteBank(Bank bank)
    {
        databaseAccess.BankList.Remove(bank);
        databaseAccess.Save();
    }

    public List<Bank> GetBanks()
    {
        return databaseAccess.BankList;
    }

    public void ChangeName(Bank bank, string newName)
    {
        bank.Name = newName;
        databaseAccess.Save();
    }

    public void ChangeAddress(Bank bank, string newAddress)
    {
        bank.Address = newAddress;
        databaseAccess.Save();
    }

    public void ChangeOwnership(Bank bank, Bank.OwnershipType newOwnership)
    {
        bank.Ownership = newOwnership;
        databaseAccess.Save();
    }

    public void RemoveDepositCondition(Bank bank, Bank.DepositType depositType)
    {
        bank.DepositConditions.Remove(depositType);
        databaseAccess.Save();
    }

    public void ChangeDepositCondition(Bank bank, Bank.DepositType depositType, int newPercent)
    {
        bank.DepositConditions[depositType] = newPercent;
        databaseAccess.Save();
    }

    public Bank? FindBestBank(Bank.DepositType depositType, int depositAmount, int depositTime)
    {
        Bank? bestBank = null;
        foreach (Bank bank in databaseAccess.BankList)
        {
            if (bank.DepositConditions.ContainsKey(depositType))
            {
                if (bestBank == null || bank.DepositConditions[depositType] > bestBank.DepositConditions[depositType])
                {
                    bestBank = bank;
                }
            }
        }
        return bestBank;
    }

    private readonly DatabaseReadWriter<Bank> databaseAccess;
}