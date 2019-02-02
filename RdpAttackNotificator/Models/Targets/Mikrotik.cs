using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using NLog;

namespace RdpAttackNotificator.Models.Targets
{
    public class Mikrotik : ILockTarget
    {
        protected TcpClient TcpClient { get; }

        protected Stream Stream { get; }

        protected String BlackList { get; }

        protected String User { get; private set; }

        protected String Password { get; private set; }

        protected String Hash { get; private set; }

        public Logger Logger { get; }

        public event EventHandler<Int64> InvalidResponseLengthDetected;


        public Mikrotik(String target, Int32 port, String blackList, String user, String password) : this(target, port, blackList)
        {
            this.Login(user, password);
        }

        public Mikrotik(String target, Int32 port, String blackList)
        {
            this.TcpClient = new TcpClient(target, port);
            this.Stream = this.TcpClient.GetStream();
            this.BlackList = blackList;
            this.Logger = LogManager.GetLogger("Mikrotik");
        }

        public void Close()
        {
            this.TcpClient.Close();
        }

        public bool AddToBlockList(string sourceIp)
        {
            this.Send("/ip/firewall/address-list/add");
            this.Send($"=address={sourceIp}");
            this.Send($"=list={this.BlackList}");
            this.Send($"=timeout=1d", true);

            var response =  this.Read()[0];
            if (response.Contains("!done"))
            {
                this.Logger.Info($"IP address {sourceIp} successfully added to address list {this.BlackList}.");
                return true;
            }

            this.Logger.Error($"IP address {sourceIp} was not added to address list {this.BlackList}.");
            return false;
        }

        public Boolean Login(string user, string password)
        {
            this.User = user;
            this.Password = password;

            this.Send("/login", true);
            this.Hash = this.Read()[0].Split(new string[] { "ret=" }, StringSplitOptions.None)[1];
            this.Send("/login");
            this.Send("=name=" + this.User);
            this.Send("=response=00" + this.EncodePassword(this.Password, this.Hash), true);
            if (Read()[0] == "!done")
            {
                this.Logger.Info($"Connection to {this.TcpClient.Client.RemoteEndPoint} with login {user} established. Session hash is {this.Hash}.");
                return true;
            }

            this.Logger.Info($"Connection to {this.TcpClient.Client.RemoteEndPoint} with login {user} faild.");
            return false;
        }


        protected void Send(string sentence, bool endOfSentence = false)
        {
            // get encoded data
            byte[] data = Encoding.ASCII.GetBytes(sentence.ToCharArray());
            // get encoded length
            byte[] length = this.EncodeValue(data.Length);
            // write length of package
            this.Stream.Write(length, 0, length.Length);
            // write package data
            this.Stream.Write(data, 0, data.Length);
            if (endOfSentence)
            {
                this.Stream.WriteByte(0);
            }
        }
        protected List<string> Read()
        {
            List<string> output = new List<string>();
            string line = String.Empty;
            byte[] tmp = new byte[4];
            while (true)
            {
                tmp[3] = (byte)this.Stream.ReadByte();
                if (tmp[3] == 0)
                {
                    output.Add(line);
                    if (line.Substring(0, 5) == "!done")
                    {
                        break;
                    }

                    line = String.Empty; ;
                    continue;
                }

                long count = 0;
                if (tmp[3] < 0x80)
                {
                    count = tmp[3];
                }
                else
                {
                    if (tmp[3] < 0xC0)
                    {
                        int tmpi = BitConverter.ToInt32(new byte[] { (byte)this.Stream.ReadByte(), tmp[3], 0, 0 }, 0);
                        count = tmpi ^ 0x8000;
                    }
                    else
                    {
                        if (tmp[3] < 0xE0)
                        {
                            tmp[2] = (byte)this.Stream.ReadByte();
                            int tmpi = BitConverter.ToInt32(new byte[] { (byte)this.Stream.ReadByte(), tmp[2], tmp[3], 0 }, 0);
                            count = tmpi ^ 0xC00000;
                        }
                        else
                        {
                            if (tmp[3] < 0xF0)
                            {
                                tmp[2] = (byte)this.Stream.ReadByte();
                                tmp[1] = (byte)this.Stream.ReadByte();
                                int tmpi = BitConverter.ToInt32(new byte[] { (byte)this.Stream.ReadByte(), tmp[1], tmp[2], tmp[3] }, 0);
                                count = tmpi ^ 0xE0000000;
                            }
                            else
                            {
                                if (tmp[3] == 0xF0)
                                {
                                    tmp[3] = (byte)this.Stream.ReadByte();
                                    tmp[2] = (byte)this.Stream.ReadByte();
                                    tmp[1] = (byte)this.Stream.ReadByte();
                                    tmp[0] = (byte)this.Stream.ReadByte();
                                    count = BitConverter.ToInt32(tmp, 0);
                                }
                                else
                                {
                                    //Error in packet reception, unknown length
                                    this.InvalidResponseLengthDetected?.Invoke(this, count);
                                    break;
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < count; i++)
                {
                    line += (Char)this.Stream.ReadByte();
                }
            }

            return output;
        }

        protected byte[] EncodeValue(int value)
        {
            if (value < 0x80)
            {
                byte[] tmp = BitConverter.GetBytes(value);
                return new byte[1] { tmp[0] };
            }
            if (value < 0x4000)
            {
                byte[] tmp = BitConverter.GetBytes(value | 0x8000);
                return new byte[2] { tmp[1], tmp[0] };
            }
            if (value < 0x200000)
            {
                byte[] tmp = BitConverter.GetBytes(value | 0xC00000);
                return new byte[3] { tmp[2], tmp[1], tmp[0] };
            }
            if (value < 0x10000000)
            {
                byte[] tmp = BitConverter.GetBytes(value | 0xE0000000);
                return new byte[4] { tmp[3], tmp[2], tmp[1], tmp[0] };
            }
            else
            {
                byte[] tmp = BitConverter.GetBytes(value);
                return new byte[5] { 0xF0, tmp[3], tmp[2], tmp[1], tmp[0] };
            }
        }

        protected string EncodePassword(string password, string hash)
        {
            byte[] hash_byte = new byte[hash.Length / 2];

            for (int i = 0; i <= hash.Length - 2; i += 2)
            {
                hash_byte[i / 2] = Byte.Parse(hash.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
            }
            byte[] data = new byte[1 + password.Length + hash_byte.Length];
            data[0] = 0;
            Encoding.ASCII.GetBytes(password.ToCharArray()).CopyTo(data, 1);
            hash_byte.CopyTo(data, 1 + password.Length);

            Byte[] dataHash;
            System.Security.Cryptography.MD5 md5;

            md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();

            dataHash = md5.ComputeHash(data);

            string navrat = String.Empty;
            foreach (byte h in dataHash)
            {
                navrat += h.ToString("x2");
            }
            return navrat;
        }
    }
}
