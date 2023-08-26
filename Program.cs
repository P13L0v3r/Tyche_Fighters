using System.Linq;
using System;
namespace TycheFighters
{
    public class Program
    {
        /*
        # Design
        - Game bounds have 256 position resolution on X and Y, so that positions can be represented with 2 bytes, either seperately or as a single ushort.
        - Characters inherit from a base class that has a few core methods that can be overriden.
        - Hit boxes are inheritor-defined with a method that checks for collision written for each class. This allows for special moves, custom attack boxes, etc.
        */

        public static Game GAME_INSTANCE;

        static void Main(string[] args)
        {
            using (GAME_INSTANCE = new Game(512, 512, "Tyche Fighters"))
            {
                GAME_INSTANCE.UpdateFrequency = 24.0;
                GAME_INSTANCE.Run();
            }
        }

        public const byte STAGE_FLOOR_HEIGHT = 32;
        public static byte[] MEM = new byte[65_536]; // 64 KB of RAM
        //public static ushort FIRST_EMPTY_INDEX = 0;
        // MEM[0] reserved for size of data being stored
        // MEM[1] reserved for sign of data being stored
        // MEM[2], MEM[3], and MEM[4] reserved for linear search
        // MEM[5] to MEM[1028] reserved for 256 addresses
        // Leaves almost 63 KB of RAM

        public static void STORE(byte index, ValueType data)
        {
            MEM[0] = 0;
            MEM[1] = 0;

            if (data == null)
            {
                return;
            }

            if (data is byte)
            {
                MEM[0] = 1;

                if (LIN_SEARCH())
                {
                    MEM[MEM[2] << 8 | MEM[3]] = (byte)data;
                }
                else
                {
                    return;
                }
            }
            else if (data is sbyte)
            {
                MEM[0] = 1;

                MEM[1] = (sbyte)data < 0 ? (byte)1 : (byte)0;

                if (LIN_SEARCH())
                {
                    MEM[(ushort)(MEM[2] << 8 | MEM[3])] = (byte)(sbyte)data;
                }
                else
                {
                    return;
                }
            }
            else if (data is ushort)
            {
                MEM[0] = 2;

                if (LIN_SEARCH())
                {
                    MEM[(ushort)(MEM[2] << 8 | MEM[3])] = (byte)((ushort)data >> 8);
                    MEM[(ushort)(MEM[2] << 8 | MEM[3]) + 1] = (byte)(ushort)data;
                }
                else
                {
                    return;
                }
            }
            else if (data is short)
            {
                MEM[0] = 2;

                MEM[1] = (short)data < 0 ? (byte)1 : (byte)0;

                if (LIN_SEARCH())
                {
                    MEM[(ushort)(MEM[2] << 8 | MEM[3])] = (byte)((short)data >> 8);
                    MEM[(ushort)(MEM[2] << 8 | MEM[3]) + 1] = (byte)(short)data;
                }
                else
                {
                    return;
                }
            }
            else if (data is uint)
            {
                MEM[0] = 4;

                if (LIN_SEARCH())
                {
                    MEM[(ushort)(MEM[2] << 8 | MEM[3])] = (byte)((uint)data >> 24);
                    MEM[(ushort)(MEM[2] << 8 | MEM[3]) + 1] = (byte)((uint)data >> 16);
                    MEM[(ushort)(MEM[2] << 8 | MEM[3]) + 2] = (byte)((uint)data >> 8);
                    MEM[(ushort)(MEM[2] << 8 | MEM[3]) + 3] = (byte)(uint)data;
                }
                else
                {
                    return;
                }
            }
            else if (data is int)
            {
                MEM[0] = 4;

                MEM[1] = (int)data < 0 ? (byte)1 : (byte)0;

                if (LIN_SEARCH())
                {
                    MEM[(ushort)(MEM[2] << 8 | MEM[3])] = (byte)((int)data >> 24);
                    MEM[(ushort)(MEM[2] << 8 | MEM[3]) + 1] = (byte)((int)data >> 16);
                    MEM[(ushort)(MEM[2] << 8 | MEM[3]) + 2] = (byte)((int)data >> 8);
                    MEM[(ushort)(MEM[2] << 8 | MEM[3]) + 3] = (byte)(int)data;
                }
                else
                {
                    return;
                }
            }
            else if (data is float)
            {
                MEM[0] = 4;

                if (LIN_SEARCH())
                {
                    byte[] array = BitConverter.GetBytes((float)data);

                    if (!BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(array);
                    }

                    MEM[(ushort)(MEM[2] << 8 | MEM[3])] = array[0];
                    MEM[(ushort)(MEM[2] << 8 | MEM[3]) + 1] = array[1];
                    MEM[(ushort)(MEM[2] << 8 | MEM[3]) + 2] = array[2];
                    MEM[(ushort)(MEM[2] << 8 | MEM[3]) + 3] = array[3];
                }
                else
                {
                    return;
                }
            }

            if (MEM[0] == 0)
            {
                return;
            }
            //uint address = (uint)(MEM[1] << 24 | MEM[0] << 16 | MEM[2] << 8 | MEM[3]);
            Console.WriteLine("Store address: " + (uint)(MEM[1] << 24 | MEM[0] << 16 | MEM[2] << 8 | MEM[3]));
            MEM[5 + index*4] = MEM[1];
            MEM[6 + index*4] = MEM[0];
            MEM[7 + index*4] = MEM[2];
            MEM[8 + index*4] = MEM[3];
        }

        public static object RECALL<T>(byte index)
        {
            Console.WriteLine("Recall address: " + ADDRESS_INDEX(index));
            //return SHORTCUT_READ(index);

            if (typeof(T) == typeof(byte))
            {
                return READ_byte(ADDRESS_INDEX(index));
            }
            else if (typeof(T) == typeof(sbyte))
            {
                return READ_sbyte(ADDRESS_INDEX(index));
            }
            else if (typeof(T) == typeof(ushort))
            {
                return READ_ushort(ADDRESS_INDEX(index));
            }
            else if (typeof(T) == typeof(short))
            {
                return READ_short(ADDRESS_INDEX(index));
            }
            else if (typeof(T) == typeof(uint))
            {
                return READ_uint(ADDRESS_INDEX(index));
            }
            else if (typeof(T) == typeof(int))
            {
                return READ_int(ADDRESS_INDEX(index));
            }
            else if (typeof(T) == typeof(float))
            {
                return READ_float(ADDRESS_INDEX(index));
            }
            else
            {
                return null;
            }
        }

        private static uint ADDRESS_INDEX(byte index)
        {
            return (uint)(MEM[5 + index*4] << 24 | MEM[6 + index*4] << 16 | MEM[7 + index*4] << 8 | MEM[8 + index*4]);
        }

        public static void CLEAN(byte index)
        {
            MEM[0] = MEM[6 + index*4];

            for (byte i = 0; i < MEM[0]; i++)
            {
                MEM[MEM[7 + index*4] << 8 | MEM[8 + index*4] + i] = 0;
            }

            MEM[5 + index*4] = 0;
        }

        public static void UPDATE<T>(byte index, T data) where T : struct
        {
            //Console.WriteLine((uint)(MEM[1028 + index*4] << 24 | MEM[1029 + index*4] << 16 | MEM[1030 + index*4] << 8 | MEM[1031 + index*4]));
            //SET(ADDRESS_INDEX(index), data);
            CLEAN(index);
            STORE(index, data);
        }

        public static ushort PackWorldCoords(byte x, byte y)
        {
            return (ushort)((x << 8) | y);
        }

        private static bool LIN_SEARCH()
        {
            // 00000100_00000101
            for (MEM[2] = 4; (ushort)(MEM[2] << 8 | MEM[3]) < MEM.Length; MEM[2]++)
            {
                for (MEM[3] = 5; (ushort)(MEM[2] << 8 | MEM[3]) < MEM.Length; MEM[3]++)
                {
                    if (MEM[(ushort)(MEM[2] << 8 | MEM[3])] == 0)
                    {
                        if ((ushort)(MEM[2] << 8 | MEM[3]) + MEM[0] >= MEM.Length)
                        {
                            return false;
                        }

                        for (MEM[4] = 0; MEM[4] < MEM[0]; MEM[4]++)
                        {
                            if (MEM[(ushort)(MEM[2] << 8 | MEM[3]) + MEM[4]] != 0)
                            {
                                return false;
                            }
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public static uint LET(ValueType data) //where T : struct
        {
            // address is FIRST_EMPTY_INDEX
            // number of bytes is sizeof(typeof(T))
            // returns address of the stored data

            MEM[0] = 0;
            MEM[1] = 0;

            if (data == null)
            {
                return 0;
            }

            if (data is byte)
            {
                MEM[0] = 1;

                if (LIN_SEARCH())
                {
                    MEM[MEM[2] << 8 | MEM[3]] = (byte)data;
                }
                else
                {
                    return 0;
                }
            }
            else if (data is sbyte)
            {
                MEM[0] = 1;

                MEM[1] = (sbyte)data < 0 ? (byte)1 : (byte)0;

                if (LIN_SEARCH())
                {
                    MEM[(ushort)(MEM[2] << 8 | MEM[3])] = (byte)(sbyte)data;
                }
                else
                {
                    return 0;
                }
            }
            else if (data is ushort)
            {
                MEM[0] = 2;

                if (LIN_SEARCH())
                {
                    MEM[(ushort)(MEM[2] << 8 | MEM[3])] = (byte)((ushort)data >> 8);
                    MEM[(ushort)(MEM[2] << 8 | MEM[3]) + 1] = (byte)(ushort)data;
                }
                else
                {
                    return 0;
                }
            }
            else if (data is short)
            {
                MEM[0] = 2;

                MEM[1] = (short)data < 0 ? (byte)1 : (byte)0;

                if (LIN_SEARCH())
                {
                    MEM[(ushort)(MEM[2] << 8 | MEM[3])] = (byte)((short)data >> 8);
                    MEM[(ushort)(MEM[2] << 8 | MEM[3]) + 1] = (byte)(short)data;
                }
                else
                {
                    return 0;
                }
            }
            else if (data is uint)
            {
                MEM[0] = 4;

                if (LIN_SEARCH())
                {
                    MEM[(ushort)(MEM[2] << 8 | MEM[3])] = (byte)((uint)data >> 24);
                    MEM[(ushort)(MEM[2] << 8 | MEM[3]) + 1] = (byte)((uint)data >> 16);
                    MEM[(ushort)(MEM[2] << 8 | MEM[3]) + 2] = (byte)((uint)data >> 8);
                    MEM[(ushort)(MEM[2] << 8 | MEM[3]) + 3] = (byte)(uint)data;
                }
                else
                {
                    return 0;
                }
            }
            else if (data is int)
            {
                MEM[0] = 4;

                MEM[1] = (int)data < 0 ? (byte)1 : (byte)0;

                if (LIN_SEARCH())
                {
                    MEM[(ushort)(MEM[2] << 8 | MEM[3])] = (byte)((int)data >> 24);
                    MEM[(ushort)(MEM[2] << 8 | MEM[3]) + 1] = (byte)((int)data >> 16);
                    MEM[(ushort)(MEM[2] << 8 | MEM[3]) + 2] = (byte)((int)data >> 8);
                    MEM[(ushort)(MEM[2] << 8 | MEM[3]) + 3] = (byte)(int)data;
                }
                else
                {
                    return 0;
                }
            }
            else if (data is float)
            {
                MEM[0] = 4;

                if (LIN_SEARCH())
                {
                    byte[] array = BitConverter.GetBytes((float)data);

                    if (!BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(array);
                    }

                    MEM[(ushort)(MEM[2] << 8 | MEM[3])] = array[0];
                    MEM[(ushort)(MEM[2] << 8 | MEM[3]) + 1] = array[1];
                    MEM[(ushort)(MEM[2] << 8 | MEM[3]) + 2] = array[2];
                    MEM[(ushort)(MEM[2] << 8 | MEM[3]) + 3] = array[3];
                }
                else
                {
                    return 0;
                }
            }

            if (MEM[0] == 0)
            {
                return 0;
            }

            return (uint)(MEM[1] << 24 | MEM[0] << 16 | MEM[2] << 8 | MEM[3]);
        }

        public static long READ(uint address)
        {
            if (address == 0)
            {
                return 0;
            }

            long result = 0;
            MEM[0] = (byte)(address >> 16); // number of bytes
            MEM[1] = (byte)(address >> 24); // sign
            
            for (byte i = 0; i < MEM[0]; i++)
            {
                result |= (uint)(MEM[(ushort)address + (MEM[0] - 1) - i] << 8 * i);
            }

            if (MEM[0] == 1)
            {
                return result - MEM[1] * byte.MaxValue - MEM[1];
            }
            else if (MEM[0] == 2)
            {
                return result - MEM[1] * ushort.MaxValue - MEM[1];
            }

            return result - MEM[1] * uint.MaxValue - MEM[1];
        }

        public static byte READ_byte(uint address)
        {
            return MEM[(ushort)address];
        }

        public static sbyte READ_sbyte(uint address)
        {
            return (sbyte)MEM[(ushort)address];
        }

        public static ushort READ_ushort(uint address)
        {
            return BitConverter.ToUInt16(MEM, (ushort)address);
        }

        public static short READ_short(uint address)
        {
            return BitConverter.ToInt16(MEM, (ushort)address);
        }

        public static uint READ_uint(uint address)
        {
            return BitConverter.ToUInt32(MEM, (ushort)address);
        }

        public static int READ_int(uint address)
        {
            return BitConverter.ToInt32(MEM, (ushort)address);
        }

        public static float READ_float(uint address)
        {
            return BitConverter.ToSingle(MEM, (ushort)address);
        }

        public static uint DEL(uint address)
        {
            MEM[0] = (byte)(address >> 16);

            for (byte i = 0; i < MEM[0]; i++)
            {
                MEM[(ushort)address + i] = 0;
            }

            return address & 0b0_11111111_11111111_11111111;
        }

        public static uint SET(uint address, ValueType data)
        {
            if (address == 0)
            {
                return 0;
            }

            MEM[0] = (byte)(address >> 16);

            MEM[1] = 0;

            if (data == null)
            {
                return 0;
            }

            if (data is byte)
            {
                if (MEM[0] != 1)
                {
                    return 0;
                }

                MEM[(ushort)address] = (byte)data;
            }
            else if (data is sbyte)
            {
                if (MEM[0] != 1)
                {
                    return 0;
                }

                MEM[1] = (sbyte)data < 0 ? (byte)1 : (byte)0;

                MEM[(ushort)address] = (byte)(sbyte)data;
            }
            else if (data is ushort)
            {
                if (MEM[0] != 2)
                {
                    return 0;
                }

                MEM[(ushort)address] = (byte)((ushort)data >> 8);
                MEM[(ushort)address + 1] = (byte)(ushort)data;
            }
            else if (data is short)
            {
                if (MEM[0] != 2)
                {
                    return 0;
                }

                MEM[1] = (short)data < 0 ? (byte)1 : (byte)0;

                MEM[(ushort)address] = (byte)((short)data >> 8);
                MEM[(ushort)address + 1] = (byte)(short)data;
            }
            else if (data is uint)
            {
                if (MEM[0] != 4)
                {
                    return 0;
                }

                MEM[(ushort)address] = (byte)((uint)data >> 24);
                MEM[(ushort)address + 1] = (byte)((uint)data >> 16);
                MEM[(ushort)address + 2] = (byte)((uint)data >> 8);
                MEM[(ushort)address + 3] = (byte)(uint)data;
            }
            else if (data is int)
            {
                if (MEM[0] != 4)
                {
                    return 0;
                }

                MEM[1] = (int)data < 0 ? (byte)1 : (byte)0;

                MEM[(ushort)address] = (byte)((int)data >> 24);
                MEM[(ushort)address + 1] = (byte)((int)data >> 16);
                MEM[(ushort)address + 2] = (byte)((int)data >> 8);
                MEM[(ushort)address + 3] = (byte)(int)data;
            }
            else if (data is float)
            {
                if (MEM[0] != 4)
                {
                    return 0;
                }

                byte[] array = BitConverter.GetBytes((float)data);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(array);
                }

                MEM[(ushort)address] = array[0];
                MEM[(ushort)address + 1] = array[1];
                MEM[(ushort)address + 2] = array[2];
                MEM[(ushort)address + 3] = array[3];
            }

            if (MEM[0] == 0)
            {
                return 0;
            }

            return (uint)(MEM[1] << 24 | MEM[0] << 16 | (ushort)address);
        }
    }
}