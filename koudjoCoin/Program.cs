using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace koudjoCoin
{
    class Program
    {
        static void Main(string[] args)
        {
            int difficulty = 2;
            if (args != null  && args.Length > 0)
            {
                int.TryParse(args[0], out difficulty);
            }
                
            BlockChain koudjochain = new BlockChain(difficulty);
            koudjochain.CreateTransaction(new Transaction("john-doe-address", "john-smith-address", 100));
            koudjochain.CreateTransaction(new Transaction("john-smith-address", "jane-doe-address", 50));
            koudjochain.CreateTransaction(new Transaction("john-smith-address", "jane-doe-address", 70));

            Console.WriteLine("Start mining ");
            koudjochain.MinePendingTransactions("maverick-address");

            Console.WriteLine(string.Format(" Balance for Maverick : {0}", koudjochain.GetBalanceForAddress("maverick-address")));

            Console.WriteLine("Start mining again");
            koudjochain.MinePendingTransactions("maverick-address");

            Console.WriteLine(string.Format(" Balance for Maverick : {0}", koudjochain.GetBalanceForAddress("maverick-address")));
            
        }
    }

    #region Models 
    public class Block
    {
        public DateTime Timestamp { get; set; }
        public List<Transaction> Transactions { get; set; }
        public string Hash { get; set; }
        public string PreviousHash { get; set; }
        public int Nonce { get; set; }
        public Block(DateTime _timestamp, List<Transaction> _transactions, string _previoushash = "")
        {            
            this.Timestamp = _timestamp;
            this.Transactions = _transactions;
            this.Hash = CalculateHash();
            this.PreviousHash = _previoushash;
        }

        public string CalculateHash()
        {
            string data = string.Concat(this.PreviousHash, this.Timestamp.ToLongDateString(), Transactions, this.Nonce.ToString());
            var sha256Provider = HashAlgorithm.Create("SHA256");
            var binHash = sha256Provider.ComputeHash(Encoding.ASCII.GetBytes(data));
            return (BitConverter.ToString(binHash)).Replace("-", "");
        }
        public void MineBlock(int difficulty)
        {
            while (this.Hash.Substring(0, difficulty) != string.Concat(Enumerable.Repeat("0", difficulty)))
            {
                this.Nonce++;
                this.Hash = this.CalculateHash();
            }
            Console.WriteLine(" Block mined " + this.Hash);
        }
    }

    public class Transaction
    {
        public int Amount { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }

        public Transaction(string _fromAddress, string _toAddress, int _amount)
        {
            this.FromAddress = _fromAddress;
            this.ToAddress = _toAddress;
            this.Amount = _amount;
        }
        public override string ToString()
        {
            return string.Concat(FromAddress, ToAddress, Amount.ToString());
        }
    }
    #endregion

    #region BlockChain
    public class BlockChain
    {
        public List<Block> Chain {get; set; }

        public int Difficulty { get; set; }

        public List<Transaction> PendingTransactions { get; set; }
        public int MiningReward { get; set; }

        public BlockChain()
        {
            Init();
        }

        public BlockChain(int difficulty)
        {
            Init();
            this.Difficulty = difficulty;
        }

        private void Init()
        {
            Difficulty = 2;
            MiningReward = 100;
            PendingTransactions = new List<Transaction>();
            Chain = new List<Block>();
            this.Chain.Add(CreateGenesisBlock());
        }
 
        public Block CreateGenesisBlock()
        {
            List<Transaction> beginning = new List<Transaction>();
            beginning.Add(new Transaction("Epoch", "Universe", 4));

            return new Block(new DateTime(), beginning, "0");
        }

        public void MinePendingTransactions(string miningRewardAddress)
        {
            Block block = new Block(DateTime.Now, this.PendingTransactions);
            block.MineBlock(this.Difficulty);

            Console.WriteLine(" Block sucessfully mined ");
            this.Chain.Add(block);

            this.PendingTransactions = new List<Transaction> { new Transaction(null, miningRewardAddress, this.MiningReward) };
        }

        public void CreateTransaction(Transaction transaction)
        {
            this.PendingTransactions.Add(transaction);
        }
        public int GetBalanceForAddress(string address)
        {
            int balance = 0;
            foreach (Block block in Chain)
            {
                foreach (Transaction transaction in block.Transactions)
                {
                    if(string.Compare(transaction.FromAddress, address) == 0)
                    {
                        balance = balance - transaction.Amount;
                    }

                    if (string.Compare(transaction.ToAddress, address) == 0)
                    {
                        balance = balance + transaction.Amount;
                    }
                }
            }

            return balance;
        }
        public bool IsValidChain()
        {
            Block[] arBlocks = this.Chain.ToArray();

            int i = 1;
            for(i=1; i < arBlocks.Length; i++)
            {
                Block currentBlock = arBlocks[i];
                Block previousBlock = arBlocks[i - 1];

                if(currentBlock.Hash != currentBlock.CalculateHash())
                {
                    return false;
                }

                if (currentBlock.PreviousHash != previousBlock.Hash)
                {
                    return false;
                }
            }
            return true;
        }
    }
    #endregion

}
