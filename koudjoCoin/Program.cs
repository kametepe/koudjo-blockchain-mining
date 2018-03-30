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
            Console.WriteLine(" Adding Block ");
            koudjochain.AddBlock(new Block(1, "15/2/2018", new CoinData {Amount = 14 }));
            Console.WriteLine(" Adding Block ");
            koudjochain.AddBlock(new Block(1, "17/2/2018", new CoinData { Amount = 20 }));
            Console.WriteLine(" Adding Block ");
            koudjochain.AddBlock(new Block(1, "18/2/2018", new CoinData { Amount = 41 }));
            //Console.WriteLine(" Current Chain ");
            //Console.WriteLine(JsonConvert.SerializeObject(koudjochain));
            //Console.WriteLine("");
            //Console.WriteLine(" Is chain valid ? " + koudjochain.IsValidChain());
        }
    }

    public class Block
    {
        public int Index { get; set; }
        public string Timestamp { get; set; }
        public CoinData Data { get; set; }
        public string Hash { get; set; }
        public string PreviousHash { get; set; }
        public int Nonce { get; set; }
        public Block(int _index, string _timestamp, CoinData _data, string _previoushash = "")
        {
            this.Index = _index;
            this.Timestamp = _timestamp;
            this.Data = _data;
            this.Hash = CalculateHash();
            this.PreviousHash = _previoushash;
        }

        public string CalculateHash()
        {
            string data = this.Index + this.Timestamp + this.Data.ToString() +  this.Nonce.ToString();
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
            Console.WriteLine(" Mining finished " + this.Hash);
        }
    }

    public class CoinData
    {
        public int Amount { get; set; }
        public override string ToString()
        {
            return Amount.ToString();
        }
    }
    public class BlockChain
    {
        public List<Block> Chain {get; set; }

        public int Difficulty { get; set; }

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
            Chain = new List<Block>();
            this.Chain.Add(CreateGenesisBlock());
        }

        public void AddBlock(Block newBlock)
        {           
            newBlock.PreviousHash = this.Chain.Last().Hash;
            newBlock.MineBlock(this.Difficulty);
            this.Chain.Add(newBlock);
        }
        public Block CreateGenesisBlock()
        {
            return new Block(0, "10/03/2018", new CoinData { Amount = 4}, "0");
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

}
