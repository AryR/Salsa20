using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salsa20
{
    public class Salsa20
    {
        private byte[] bytesCollector;
        private int bytesCollectorIndexCount;

        private byte[] key;
        private byte[] nonce;

        private byte[] constant;

        public EventHandler<byte[]> BytesEncripted;
        public EventHandler<byte[]> BytesDecripted;

        public Salsa20(string key, string nonce)
        {
            bytesCollector = new byte[64];
            bytesCollectorIndexCount = 0;

            this.key = Encoding.ASCII.GetBytes(key);
            this.nonce = Encoding.ASCII.GetBytes(nonce);

            if ( key.Length != 16 && key.Length != 32)
                throw new Exception("Key must have be 128 or 256 bits");
            if (nonce.Length != 8)
                throw new Exception("Nonce must have be 64 bits");

            if(key.Length == 32)
                this.constant = Encoding.ASCII.GetBytes("expand 32-byte k");
            else
                this.constant = Encoding.ASCII.GetBytes("expand 16-byte k");
        }

        /// <summary>
        /// Encrypt message of 64 bytes. At finish encrypt event BytesEncripted will be handled
        /// </summary>
        /// <param name="newbyte">Byte to encrypt</param>
        public void Encrypt (byte newbyte)
        {
            bytesCollector[bytesCollectorIndexCount] = newbyte;

            if(bytesCollectorIndexCount < 63)
                bytesCollectorIndexCount++;
            else
            {
                Encrypt(this.bytesCollector).ContinueWith(x => BytesEncripted?.Invoke(this, x.Result));
                bytesCollector = new byte[64];
                bytesCollectorIndexCount = 0;
            }
        }

        /// <summary>
        /// Decrypt message of 64 bytes. At finish decrypt event BytesDecripted will be handled
        /// </summary>
        /// <param name="bytes"></param>
        public void Decrypt(byte[] bytes)
        {
            Encrypt(bytes).ContinueWith(x => BytesDecripted?.Invoke(this, x.Result));
        }

        /// <summary>
        /// Encrypt/Decrypt message.
        /// </summary>
        /// <param name="bytes">Message to encrypt/decrypt</param>
        /// <returns>Task with byte array result</returns>
        async private Task<byte[]> Encrypt(byte[] bytes)
        {
            //Message to encrypt
            uint[] message = new uint[16];
            for (int i = 0; i < 16; i++)
                message[i] = LittleEndian(bytes, 4 * i);

            //Create vector whit words
            uint[] vector = Expand20();
            
            //Hash vector
            vector = Hash(vector);

            //Xor hashed vector and message
            for (int i = 0; i < 16; i++)
                vector[i] = vector[i] ^ message[i];

            List<byte> encriptedBytes = new List<byte>();

            //Convert vector to byte array
            for (int i = 0; i < 16; i++)
                encriptedBytes.InsertRange(encriptedBytes.Count, InverseLittleEndian(vector[i]));

            return encriptedBytes.ToArray();
        }

        /*Byte Operands:
            &   Binary AND Operator copies a bit to the result if it exists in both operands.
            |   Binary OR Operator copies a bit if it exists in either operand.
            ^   Binary XOR Operator copies the bit if it is set in one operand but not both.
            ~   Binary Ones Complement Operator is unary and has the effect of 'flipping' bits.
            <<  Binary Left Shift Operator. The left operands value is moved left by the number of bits specified by the right operand.
            >>  Binary Right Shift Operator. The left operands value is moved right by the number of bits specified by the right operand.
        */

        /// <summary>
        /// Create vector with 16 positions of 32 bits words (unit uses 32 bits). 1 word = 4 bytes
        /// </summary>
        /// <returns>Word's vector</returns>
        private uint[] Expand20()
        {
            uint[] vector = new uint[16];

            vector[0] = LittleEndian(constant, 0);
            vector[5] = LittleEndian(constant, 4);
            vector[10] = LittleEndian(constant, 8);
            vector[15] = LittleEndian(constant, 12);

            vector[1] = LittleEndian(key, 0);
            vector[2] = LittleEndian(key, 4);
            vector[3] = LittleEndian(key, 8);
            vector[4] = LittleEndian(key, 12);

            int keyIndex = key.Length == 32 ? 16 : 0;
            vector[11] = LittleEndian(key, keyIndex + 0);
            vector[12] = LittleEndian(key, keyIndex + 4);
            vector[13] = LittleEndian(key, keyIndex + 8);
            vector[14] = LittleEndian(key, keyIndex + 12);

            vector[6] = LittleEndian(nonce, 0);
            vector[7] = LittleEndian(nonce, 4);

            //Stream position. Not used for this example
            //vector[8] = LittleEndian(position, 0);
            //vector[9] = LittleEndian(position, 4);

            return vector;
        }

        /// <summary>
        /// Salsa20 Hash function
        /// </summary>
        /// <param name="vector">Vector to hash</param>
        /// <returns>Hashed vector</returns>
        private uint[] Hash(uint[] vector)
        {
            uint[] encrypted = (uint[])vector.Clone();

            //Apply 10 double rounds
            for (int i = 0; i < 10; i++)
                encrypted = Doubleround(encrypted);

            //Sum hashed vector and original vector
            for (int i = 0; i < 16; i++)
                encrypted[i] = encrypted[i] + vector[i];

            return encrypted;
        }

        /// <summary>
        /// Convert byte array to uint
        /// </summary>
        /// <param name="input">Data to convert</param>
        /// <param name="inputOffset">Offset of data</param>
        /// <returns>Data converted</returns>
        private static uint LittleEndian(byte[] input, int inputOffset)
        {
            return (uint)(((input[inputOffset] //Copy first 8 bits
                | (input[inputOffset + 1] << 8)) //Move 8 bits and copy next
                | (input[inputOffset + 2] << 16))
                | (input[inputOffset + 3] << 24));
        }

        /// <summary>
        /// Convert uint to byte array
        /// </summary>
        /// <param name="input">Data to convert</param>
        /// <returns>Data converted</returns>
        private static byte[] InverseLittleEndian(uint input)
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)input; //Copy first 8 bits
            bytes[1] = (byte)(input >> 8); //Move 8 bits and copy next
            bytes[2] = (byte)(input >> 16);
            bytes[3] = (byte)(input >> 24);
            return bytes;
        }

        /// <summary>
        /// Column-round + Row-round
        /// </summary>
        /// <param name="vector">Vector to round</param>
        /// <returns>vector rounded</returns>
        private uint[] Doubleround(uint[] vector)
        {
            return Rowround(Columnround(vector));
        }

        /// <summary>
        /// If x is a 16-word input: x = (x0, x1, x2, ..., x15)
        /// then the function can be defined as follow: rowround(x) = (y0, y1, y2, ..., y15) where:
        /// (y0, y1, y2, y3) = quarterround(x0, x1, x2, x3)
        /// (y5, y6, y7, y4) = quarterround(x5, x6, x7, x4)
        /// (y10, y11, y8, y9) = quarterround(x10, x11, x8, x9)
        /// (y15, y12, y13, y14) = quarterround(x15, x12, x13, x14)
        /// </summary>
        /// <param name="vector">Vector to row-round</param>
        /// <returns>vector row-rounded</returns>
        private uint[] Rowround(uint[] vector)
        {
            uint[] matrixRounded = new uint[16];
            uint[] rowRounded = new uint[4];

            rowRounded = Quarterround(vector[0], vector[1], vector[2], vector[3]);
            matrixRounded[0] = rowRounded[0];
            matrixRounded[1] = rowRounded[1];
            matrixRounded[2] = rowRounded[2];
            matrixRounded[3] = rowRounded[3];

            rowRounded = Quarterround(vector[5], vector[6], vector[7], vector[4]);
            matrixRounded[5] = rowRounded[0];
            matrixRounded[6] = rowRounded[1];
            matrixRounded[7] = rowRounded[2];
            matrixRounded[4] = rowRounded[3];

            rowRounded = Quarterround(vector[10], vector[11], vector[8], vector[9]);
            matrixRounded[10] = rowRounded[0];
            matrixRounded[11] = rowRounded[1];
            matrixRounded[8] = rowRounded[2];
            matrixRounded[9] = rowRounded[3];

            rowRounded = Quarterround(vector[15], vector[12], vector[13], vector[14]);
            matrixRounded[15] = rowRounded[0];
            matrixRounded[12] = rowRounded[1];
            matrixRounded[13] = rowRounded[2];
            matrixRounded[14] = rowRounded[3];

            return matrixRounded;
        }

        /// <summary>
        /// If x is a 16-word input: x = (x0, x1, x2, ..., x15) 
        /// then the function can be defined as follow: columnround(x) = (y0, y1, y2, ..., y15) where:
        /// (y0, y4, y8, y12) = quarterround(x0, x4, x8, x12)
        /// (y5, y9, y13, y1) = quarterround(x5, x9, x13, x1)
        /// (y10, y14, y2, y6) = quarterround(x10, x14, x2, x6)
        /// (y15, y3, y7, y11) = quarterround(x15, x3, x7, x11)
        /// </summary>
        /// <param name="vector">Vector to column-round</param>
        /// <returns>vector column-rounded</returns>
        private uint[] Columnround(uint[] vector)
        {
            uint[] matrixRounded = new uint[16];
            uint[] columnRounded = new uint[4];

            columnRounded = Quarterround(vector[0], vector[4], vector[8], vector[12]);
            matrixRounded[0] = columnRounded[0];
            matrixRounded[4] = columnRounded[1];
            matrixRounded[8] = columnRounded[2];
            matrixRounded[12] = columnRounded[3];

            columnRounded = Quarterround(vector[5], vector[9], vector[13], vector[1]);
            matrixRounded[5] = columnRounded[0];
            matrixRounded[9] = columnRounded[1];
            matrixRounded[13] = columnRounded[2];
            matrixRounded[1] = columnRounded[3];

            columnRounded = Quarterround(vector[10], vector[14], vector[2], vector[6]);
            matrixRounded[10] = columnRounded[0];
            matrixRounded[14] = columnRounded[1];
            matrixRounded[2] = columnRounded[2];
            matrixRounded[6] = columnRounded[3];

            columnRounded = Quarterround(vector[15], vector[3], vector[7], vector[11]);
            matrixRounded[15] = columnRounded[0];
            matrixRounded[3] = columnRounded[1];
            matrixRounded[7] = columnRounded[2];
            matrixRounded[11] = columnRounded[3];

            return matrixRounded;
        }

        /// <summary>
        /// If x is a 4-word input: x = (x0, x1, x2, x3)
        /// then the function can be defined as follow: quarterround(x) = (y0, y1, y2, y3) where:
        /// y1 = x1 XOR (left-rotation(x0 + x3, 7))
        /// y2 = x2 XOR (left-rotation(y1 + x0, 9))
        /// y3 = x3 XOR (left-rotation(y2 + y1, 13))
        /// y0 = x0 XOR (left-rotation(y3 + y2, 18))
        /// </summary>
        /// <param name="a">First word</param>
        /// <param name="b">Second word</param>
        /// <param name="c">Third word</param>
        /// <param name="d">Fourth word</param>
        /// <returns>Quarterd word</returns>
        private uint[] Quarterround(uint a, uint b, uint c, uint d)
        {
            uint[] vector = new uint[4];
            vector[1] = b ^ BinaryLeftRotation(a + d, 7);
            vector[2] = c ^ BinaryLeftRotation(vector[1] + a, 9);
            vector[3] = d ^ BinaryLeftRotation(vector[2] + vector[1], 13);
            vector[0] = a ^ BinaryLeftRotation(vector[3] + vector[2], 18);

            return vector;
        }

        /// <summary>
        /// The a-bit left rotation of a 4-byte word w is denoted in the algorithm as w &lt&lt&lt a. The leftmost a bits move to the rightmost positions.
        /// </summary>
        /// <param name="w">word</param>
        /// <param name="a">bits to rotate</param>
        /// <returns>rotated word</returns>
        private uint BinaryLeftRotation(uint w, int a)
        {
            return ((w) << (a)) //Move a bits of the w
                | ((w) >> (32 - (a))); //Copy a bits at the end of w
        }
    }
}
