using System.Data.Common;
using static TycheFighters.Program;

namespace TycheFighters
{
    public struct Triangle
    {
        public byte[] worldCoords = new byte[6];

        private ushort v1;
        private ushort v2;
        private ushort v3;
        public byte color;

        public Triangle(ushort v1, ushort v2, ushort v3, byte color = 255)
        {
            worldCoords[0] = (byte)(v1 >> 8);         // x1
            worldCoords[1] = (byte)(v1 & 0b11111111); // y1

            worldCoords[2] = (byte)(v2 >> 8);         // x2
            worldCoords[3] = (byte)(v2 & 0b11111111); // y2

            worldCoords[4] = (byte)(v3 >> 8);         // x3
            worldCoords[5] = (byte)(v3 & 0b11111111); // y3

            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;

            this.color = color;
        }

        public bool Contains(ushort p)
        {
            ushort A = TwiceArea();
            ushort A1 = new Triangle(p, v1, v2).TwiceArea();
            ushort A2 = new Triangle(p, v2, v3).TwiceArea();
            ushort A3 = new Triangle(p, v3, v1).TwiceArea();

            return A1 + A2 + A3 == A;
        }

        public static bool Intersect(Triangle A, Triangle B)
        {
            for (byte i = 0; i < 3; i++)
            {
                for (byte j = 0; j < 3; j++)
                {
                    // P1 and P2
                    byte x1 = A.worldCoords[i*2];
                    byte y1 = A.worldCoords[i*2+1];
                    byte x2 = A.worldCoords[0];
                    byte y2 = A.worldCoords[1];
                    if (i < 2) // Account for indexing
                    {
                        x2 = A.worldCoords[i*2+2];
                        y2 = A.worldCoords[i*2+3];
                    }

                    // P3 and P4
                    byte x3 = B.worldCoords[j*2];
                    byte y3 = B.worldCoords[j*2+1];
                    byte x4 = B.worldCoords[0];
                    byte y4 = B.worldCoords[1];
                    if (j < 2) // Account for indexing
                    {
                        x4 = B.worldCoords[j*2+2];
                        y4 = B.worldCoords[j*2+3];
                    }

                    // Solve for denominator of U
                    short Ud = (short)((y4-y3)*(x2-x1)-(x4-x3)*(y2-y1));

                    // If denominator is 0 then the lines are parallel
                    if (Ud == 0)
                    {
                        continue;
                    }

                    // Solve for numerators of Ua and Ub
                    short Ua = (short)((x4-x3)*(y1-y3)-(y4-y3)*(x1-x3));
                    short Ub = (short)((x2-x1)*(y1-y3)-(y2-y1)*(x1-x3));

                    // If Ua/Ud and Ub/Ud are between 0 and 1 then the lines intersect
                    if (0 <= Ua && Ua <= Ud && 0 <= Ub && Ub <= Ud)
                    {
                        return true;
                    }
                }
            }

            // If no lines intersect, the triangles don't intersect
            return false;
        }

        public ushort TwiceArea()
        {
            return (ushort)Math.Abs(worldCoords[0] * (worldCoords[3] - worldCoords[5]) + worldCoords[2] * (worldCoords[5] - worldCoords[1]) + worldCoords[4] * (worldCoords[1] - worldCoords[3]));
        }

        public Triangle Flip(byte width)
        {
            return new Triangle((ushort)((byte)(width - (byte)(v1 >> 8)) << 8 | (byte)v1), (ushort)((byte)(width - (byte)(v2 >> 8)) << 8 | (byte)v2), (ushort)((byte)(width - (byte)(v3 >> 8)) << 8 | (byte)v3), color);
        }

        public static Triangle operator +(Triangle a, ushort offset) => new Triangle((ushort)(a.v1+offset), (ushort)(a.v2+offset), (ushort)(a.v3+offset), a.color);
        public static Triangle operator -(Triangle a, ushort offset) => new Triangle((ushort)(a.v1-offset), (ushort)(a.v2-offset), (ushort)(a.v3-offset), a.color);
    }

    public struct Line
    {
        private ushort p1;
        private ushort p2;
        private byte thickness;

        public Line(ushort p1, ushort p2, byte thickness)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.thickness = thickness;
        }

        public List<Triangle> Draw()
        {
            List<Triangle> triangles = new List<Triangle>();

            byte x1 = (byte)(p1 >> 8);
            byte y1 = (byte)(p1 & 0b11111111);

            byte x2 = (byte)(p2 >> 8);
            byte y2 = (byte)(p2 & 0b11111111);

            float vx = x1 - x2;
            float vy = y1 - y2;

            vx /= Py(vx, vy);
            vy /= Py(vx, vy);

            // tip triangles
            triangles.Add(
                new Triangle
                (
                    PackWorldCoords((byte)(x1 + vx * thickness), (byte)(y1 + vy * thickness)),
                    PackWorldCoords((byte)(x1 + vy * thickness), (byte)(y1 - vx * thickness)),
                    PackWorldCoords((byte)(x1 - vy * thickness), (byte)(y1 + vx * thickness))
                )
            );

            triangles.Add(
                new Triangle
                (
                    PackWorldCoords((byte)(x2 - vx * thickness), (byte)(y2 - vy * thickness)),
                    PackWorldCoords((byte)(x2 - vy * thickness), (byte)(y2 + vx * thickness)),
                    PackWorldCoords((byte)(x2 + vy * thickness), (byte)(y2 - vx * thickness))
                )
            );

            // body triangles
            triangles.Add(
                new Triangle
                (
                    PackWorldCoords((byte)(x2 - vy * thickness), (byte)(y2 + vx * thickness)),
                    PackWorldCoords((byte)(x1 + vy * thickness), (byte)(y1 - vx * thickness)),
                    PackWorldCoords((byte)(x1 - vy * thickness), (byte)(y1 + vx * thickness))
                )
            );

            triangles.Add(
                new Triangle
                (
                    PackWorldCoords((byte)(x1 + vy * thickness), (byte)(y1 - vx * thickness)),
                    PackWorldCoords((byte)(x2 - vy * thickness), (byte)(y2 + vx * thickness)),
                    PackWorldCoords((byte)(x2 + vy * thickness), (byte)(y2 - vx * thickness))
                )
            );

            return triangles;
        }

        private float Py(float a, float b)
        {
            return MathF.Sqrt(a * a + b * b);
        }
    }

    public class Bar : IDrawable
    {
        public ushort position { get; set; }
        public readonly byte width;
        public readonly byte height;

        public byte value;
        public byte maxValue;
        public bool reversed;

        public Bar(ushort position, byte maxValue, byte width = 16, byte height = 4, bool reversed = false)
        {
            this.position = position;
            this.maxValue = maxValue;
            this.width = width;
            this.height = height;
            this.reversed = reversed;
        }

        public List<Triangle> Draw()
        {
            if (reversed)
            {
                return new List<Triangle>()
                {
                    new Triangle(position, (ushort)(position - (width << 8)), (ushort)(position - (width << 8 | height)), color : 128),
                    new Triangle(position, (ushort)(position - height), (ushort)(position - (width << 8 | height)), color: 128),

                    new Triangle(position, (ushort)(position - (value*width/maxValue << 8)), (ushort)(position - (value*width/maxValue << 8 | height)), color : 255),
                    new Triangle(position, (ushort)(position - height), (ushort)(position - (value*width/maxValue << 8 | height)), color: 255),
                };
            }

            return new List<Triangle>()
            {
                new Triangle(position, (ushort)(position + (width << 8)), (ushort)(position + (width << 8 | height)), color : 128),
                new Triangle(position, (ushort)(position + height), (ushort)(position + (width << 8 | height)), color: 128),

                new Triangle(position, (ushort)(position + (value*width/maxValue << 8)), (ushort)(position + (value*width/maxValue << 8 | height)), color : 255),
                new Triangle(position, (ushort)(position + height), (ushort)(position + (value*width/maxValue << 8 | height)), color: 255),
            };
        }
    }

    public class Burst : IInstance
    {
        public ushort position { get; set; }
        public byte age { get; set; } = 0;
        public byte lifeSpan { get; set; }

        public Burst(ushort position, byte lifeSpan)
        {
            this.position = PackWorldCoords(Math.Clamp((byte)(position >> 8), (byte)0, (byte)243), Math.Clamp((byte)position, (byte)0, (byte)243));
            this.lifeSpan = lifeSpan;
        }

        private List<Triangle> frame = new List<Triangle>()
        {
            new Triangle(2054,2570,1544,color:255),
            new Triangle(1540,2054,1544,color:255),
            new Triangle(1540,2562,2054,color:255),
            new Triangle(514,1540,1030,color:255),
new Triangle(522,1030,1544,color:255),
new Triangle(1030,1540,1544,color:255),

new Triangle(1797,3078,1799,color:255),
new Triangle(1285,1536,1797,color:255),
new Triangle(6,1285,1287,color:255),
new Triangle(1548,1287,1799,color:255),

new Triangle(1798,2056,1543,color:0),
new Triangle(1541,2052,1798,color:0),
new Triangle(1028,1541,1286,color:0),
new Triangle(1032,1286,1543,color:0),

new Triangle(1797,2310,1799,color:0),
new Triangle(1285,1797,1799,color:0),
new Triangle(1285,1539,1797,color:0),
new Triangle(774,1285,1287,color:0),
new Triangle(1545,1287,1799,color:0),
new Triangle(1287,1285,1799,color:0),
            };

        public List<Triangle> Draw()
        {
            List<Triangle> triangles = new List<Triangle>();
            foreach (Triangle triangle in frame)
            {
                triangles.Add(triangle + position);
            }
            return triangles;
        }

        public bool IsDead()
        {
            if (age < lifeSpan)
            {
                age++;
                return false;
            }

            return true;
        }
    }

    public class Text : IDrawable
    {
        public ushort position { get; set; }
        public string text;
        public byte size;

        private List<Triangle> triangles;

        public Text(string text, ushort position, byte size = 1)
        {
            this.text = text;
            this.position = position;
            this.size = size;
            this.triangles = new List<Triangle>();
        }

        public List<Triangle> Draw()
        {
            triangles = new List<Triangle>();

            for (byte i = 0; i < text.Length; i++)
            {
                switch(text[i])
                {
                    case '0':
                        Add_0(i);
                        break;
                    case '1':
                        Add_1(i);
                        break;
                    case '2':
                        Add_2(i);
                        break;
                    case '3':
                        Add_3(i);
                        break;
                    case '4':
                        Add_4(i);
                        break;
                    case '5':
                        Add_5(i);
                        break;
                    case '6':
                        Add_6(i);
                        break;
                    case '7':
                        Add_7(i);
                        break;
                    case '8':
                        Add_8(i);
                        break;
                    case '9':
                        Add_9(i);
                        break;
                    case '.':
                        Add_period(i);
                        break;
                    case '%':
                        Add_percent(i);
                        break;
                    case 'A':
                        Add_A(i);
                        break;
                    case 'B':
                        Add_B(i);
                        break;
                    case 'C':
                        Add_C(i);
                        break;
                    case 'D':
                        Add_D(i);
                        break;
                    case 'E':
                        Add_E(i);
                        break;
                    case 'F':
                        Add_F(i);
                        break;
                    case 'G':
                        Add_G(i);
                        break;
                    case 'H':
                        Add_H(i);
                        break;
                    case 'I':
                        Add_I(i);
                        break;
                    case 'J':
                        Add_J(i);
                        break;
                    case 'K':
                        Add_K(i);
                        break;
                    case 'L':
                        Add_L(i);
                        break;
                    case 'M':
                        Add_M(i);
                        break;
                    case 'N':
                        Add_N(i);
                        break;
                    case 'O':
                        Add_O(i);
                        break;
                    case 'P':
                        Add_P(i);
                        break;
                    case 'Q':
                        Add_Q(i);
                        break;
                    case 'R':
                        Add_R(i);
                        break;
                    case 'S':
                        Add_S(i);
                        break;
                    case 'T':
                        Add_T(i);
                        break;
                    case 'U':
                        Add_U(i);
                        break;
                    case 'V':
                        Add_V(i);
                        break;
                    case 'W':
                        Add_W(i);
                        break;
                    case 'X':
                        Add_X(i);
                        break;
                    case 'Y':
                        Add_Y(i);
                        break;
                    case 'Z':
                        Add_Z(i);
                        break;
                }
            }

            for (ushort i = 0; i < triangles.Count; i++)
            {
                triangles[i] += position;
            }

            return triangles;
        }

        private void Add_0(byte index)
        {
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(10 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(1547 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1 * size), (ushort)(514 * size), (ushort)(10 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(1034 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1538 * size), (ushort)(1547 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1025 * size), (ushort)(1538 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(514 * size), (ushort)(1 * size), (ushort)(512 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(512 * size), (ushort)(1025 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(266 * size), (ushort)(523 * size), (ushort)(1282 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1025 * size), (ushort)(266 * size), (ushort)(1282 * size), color: 255) + (ushort)(index * size * 7 << 8));
        }

        private void Add_1(byte index)
        {
            triangles.Add(new Triangle((ushort)(1035 * size), (ushort)(524 * size), (ushort)(1025 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(512 * size), (ushort)(1025 * size), (ushort)(524 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(524 * size), (ushort)(11 * size), (ushort)(522 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(11 * size), (ushort)(9 * size), (ushort)(522 * size), color: 255) + (ushort)(index * size * 7 << 8));
        }

        private void Add_2(byte index)
        {
            triangles.Add(new Triangle((ushort)(512 * size), (ushort)(1538 * size), (ushort)(1 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1538 * size), (ushort)(1540 * size), (ushort)(1 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1547 * size), (ushort)(1034 * size), (ushort)(1545 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1547 * size), (ushort)(1036 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(1034 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1 * size), (ushort)(514 * size), (ushort)(3 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(3 * size), (ushort)(514 * size), (ushort)(516 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1545 * size), (ushort)(1034 * size), (ushort)(1032 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(516 * size), (ushort)(1543 * size), (ushort)(1032 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1543 * size), (ushort)(1545 * size), (ushort)(1032 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(5 * size), (ushort)(3 * size), (ushort)(516 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(5 * size), (ushort)(516 * size), (ushort)(1032 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(10 * size), (ushort)(521 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(521 * size), (ushort)(10 * size), (ushort)(8 * size), color: 255) + (ushort)(index * size * 7 << 8));
        }

        private void Add_3(byte index)
        {
            triangles.Add(new Triangle((ushort)(1025 * size), (ushort)(1538 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(3 * size), (ushort)(1 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(514 * size), (ushort)(1 * size), (ushort)(512 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1547 * size), (ushort)(1034 * size), (ushort)(1538 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(516 * size), (ushort)(3 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1025 * size), (ushort)(514 * size), (ushort)(512 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(10 * size), (ushort)(8 * size), (ushort)(521 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(10 * size), (ushort)(521 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(1547 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1036 * size), (ushort)(523 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(517 * size), (ushort)(1030 * size), (ushort)(775 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(262 * size), (ushort)(517 * size), (ushort)(775 * size), color: 255) + (ushort)(index * size * 7 << 8));
        }

        private void Add_4(byte index)
        {
            triangles.Add(new Triangle((ushort)(1537 * size), (ushort)(1548 * size), (ushort)(1035 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(517 * size), (ushort)(519 * size), (ushort)(1030 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(6 * size), (ushort)(517 * size), (ushort)(11 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1024 * size), (ushort)(1537 * size), (ushort)(1035 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(11 * size), (ushort)(517 * size), (ushort)(524 * size), color: 255) + (ushort)(index * size * 7 << 8));
        }

        private void Add_5(byte index)
        {
            triangles.Add(new Triangle((ushort)(1 * size), (ushort)(514 * size), (ushort)(3 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(1547 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(516 * size), (ushort)(3 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(1034 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1545 * size), (ushort)(1547 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(514 * size), (ushort)(1 * size), (ushort)(512 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(512 * size), (ushort)(1025 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(518 * size), (ushort)(1029 * size), (ushort)(1031 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1538 * size), (ushort)(1542 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1025 * size), (ushort)(1538 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1542 * size), (ushort)(1031 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(524 * size), (ushort)(11 * size), (ushort)(518 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(7 * size), (ushort)(518 * size), (ushort)(11 * size), color: 255) + (ushort)(index * size * 7 << 8));
        }

        private void Add_6(byte index)
        {
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(10 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1545 * size), (ushort)(1547 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(1547 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1 * size), (ushort)(514 * size), (ushort)(10 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1032 * size), (ushort)(1545 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(1034 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1538 * size), (ushort)(1542 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1029 * size), (ushort)(1542 * size), (ushort)(1031 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1025 * size), (ushort)(1538 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(518 * size), (ushort)(1029 * size), (ushort)(1031 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(514 * size), (ushort)(1 * size), (ushort)(512 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(512 * size), (ushort)(1025 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
        }

        private void Add_7(byte index)
        {
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(10 * size), (ushort)(521 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(10 * size), (ushort)(8 * size), (ushort)(521 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1547 * size), (ushort)(1034 * size), (ushort)(1545 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(512 * size), (ushort)(1025 * size), (ushort)(516 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(516 * size), (ushort)(1025 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1545 * size), (ushort)(1034 * size), (ushort)(1032 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1029 * size), (ushort)(1545 * size), (ushort)(1032 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(516 * size), (ushort)(1029 * size), (ushort)(1032 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(1036 * size), (ushort)(523 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1547 * size), (ushort)(1036 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
        }

        private void Add_8(byte index)
        {
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(10 * size), (ushort)(520 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(7 * size), (ushort)(520 * size), (ushort)(10 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1544 * size), (ushort)(1547 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1031 * size), (ushort)(1544 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(518 * size), (ushort)(5 * size), (ushort)(512 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1 * size), (ushort)(512 * size), (ushort)(5 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1538 * size), (ushort)(1542 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1025 * size), (ushort)(1538 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1036 * size), (ushort)(1034 * size), (ushort)(523 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1025 * size), (ushort)(514 * size), (ushort)(512 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(520 * size), (ushort)(7 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1029 * size), (ushort)(1542 * size), (ushort)(520 * size), color: 255) + (ushort)(index * size * 7 << 8));
        }

        private void Add_9(byte index)
        {
            triangles.Add(new Triangle((ushort)(1538 * size), (ushort)(1547 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(517 * size), (ushort)(519 * size), (ushort)(1030 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(6 * size), (ushort)(517 * size), (ushort)(10 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1025 * size), (ushort)(1538 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(523 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(2 * size), (ushort)(512 * size), (ushort)(515 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(512 * size), (ushort)(514 * size), (ushort)(1025 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1 * size), (ushort)(512 * size), (ushort)(2 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(10 * size), (ushort)(517 * size), (ushort)(523 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_period(byte index)
        {
            triangles.Add(new Triangle((ushort)(512 * size), (ushort)(1025 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(514 * size), (ushort)(1025 * size), (ushort)(1027 * size), color: 255) + (ushort)(index * size * 7 << 8));
        }

        private void Add_percent(byte index)
        {
            triangles.Add(new Triangle((ushort)(769 * size), (ushort)(1282 * size), (ushort)(771 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(771 * size), (ushort)(1282 * size), (ushort)(1284 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(264 * size), (ushort)(777 * size), (ushort)(266 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(266 * size), (ushort)(777 * size), (ushort)(779 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(0 * size), (ushort)(1546 * size), (ushort)(2 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(2 * size), (ushort)(1546 * size), (ushort)(1548 * size), color: 255) + (ushort)(index * size * 7 << 8));
        }

        private void Add_A(byte index)
        {
            triangles.Add(new Triangle((ushort)(513 * size), (ushort)(523 * size), (ushort)(10 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1537 * size), (ushort)(1547 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(1547 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(0 * size), (ushort)(513 * size), (ushort)(10 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1024 * size), (ushort)(1537 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(1034 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(518 * size), (ushort)(517 * size), (ushort)(1030 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1031 * size), (ushort)(518 * size), (ushort)(1030 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_B(byte index)
        {
            triangles.Add(new Triangle((ushort)(524 * size), (ushort)(11 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1544 * size), (ushort)(1547 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(1547 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1 * size), (ushort)(514 * size), (ushort)(11 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1031 * size), (ushort)(1544 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(1034 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1538 * size), (ushort)(1542 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1029 * size), (ushort)(1542 * size), (ushort)(1031 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1025 * size), (ushort)(1538 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(518 * size), (ushort)(1029 * size), (ushort)(1031 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(514 * size), (ushort)(1 * size), (ushort)(512 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(512 * size), (ushort)(1025 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_C(byte index)
        {
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(10 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1545 * size), (ushort)(1547 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(1547 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1 * size), (ushort)(514 * size), (ushort)(10 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1032 * size), (ushort)(1545 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(1034 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1538 * size), (ushort)(1540 * size), (ushort)(1027 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1025 * size), (ushort)(1538 * size), (ushort)(1027 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(514 * size), (ushort)(1 * size), (ushort)(512 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(512 * size), (ushort)(1025 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_D(byte index)
        {
            triangles.Add(new Triangle((ushort)(524 * size), (ushort)(11 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1538 * size), (ushort)(1547 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(1547 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1 * size), (ushort)(514 * size), (ushort)(11 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1025 * size), (ushort)(1538 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(1034 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(514 * size), (ushort)(1 * size), (ushort)(512 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(512 * size), (ushort)(1025 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_E(byte index)
        {
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(10 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1545 * size), (ushort)(1547 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(1547 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1 * size), (ushort)(514 * size), (ushort)(10 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1032 * size), (ushort)(1545 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(1034 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1538 * size), (ushort)(1540 * size), (ushort)(1027 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1025 * size), (ushort)(1538 * size), (ushort)(1027 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(514 * size), (ushort)(1 * size), (ushort)(512 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(512 * size), (ushort)(1025 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(773 * size), (ushort)(1286 * size), (ushort)(1031 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(518 * size), (ushort)(773 * size), (ushort)(1031 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_F(byte index)
        {
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(10 * size), (ushort)(513 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1545 * size), (ushort)(1547 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(1547 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(0 * size), (ushort)(513 * size), (ushort)(10 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1032 * size), (ushort)(1545 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(1034 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(773 * size), (ushort)(1286 * size), (ushort)(1031 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(518 * size), (ushort)(773 * size), (ushort)(1031 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_G(byte index)
        {
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(10 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1545 * size), (ushort)(1547 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(1547 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1 * size), (ushort)(514 * size), (ushort)(10 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1032 * size), (ushort)(1545 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(1034 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1538 * size), (ushort)(1542 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1029 * size), (ushort)(1542 * size), (ushort)(1287 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1025 * size), (ushort)(1538 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(774 * size), (ushort)(1029 * size), (ushort)(1287 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(514 * size), (ushort)(1 * size), (ushort)(512 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(512 * size), (ushort)(1025 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_H(byte index)
        {
            triangles.Add(new Triangle((ushort)(513 * size), (ushort)(524 * size), (ushort)(11 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1537 * size), (ushort)(1548 * size), (ushort)(1035 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(0 * size), (ushort)(513 * size), (ushort)(11 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1024 * size), (ushort)(1537 * size), (ushort)(1035 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(518 * size), (ushort)(517 * size), (ushort)(1030 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1031 * size), (ushort)(518 * size), (ushort)(1030 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_I(byte index)
        {
            triangles.Add(new Triangle((ushort)(1036 * size), (ushort)(523 * size), (ushort)(1025 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(512 * size), (ushort)(1025 * size), (ushort)(523 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(512 * size), (ushort)(1 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1027 * size), (ushort)(1025 * size), (ushort)(1538 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(10 * size), (ushort)(521 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(1036 * size), (ushort)(1547 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1027 * size), (ushort)(1538 * size), (ushort)(1540 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(10 * size), (ushort)(8 * size), (ushort)(521 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_J(byte index)
        {
            triangles.Add(new Triangle((ushort)(516 * size), (ushort)(3 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(521 * size), (ushort)(10 * size), (ushort)(523 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1 * size), (ushort)(514 * size), (ushort)(3 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(521 * size), (ushort)(523 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1538 * size), (ushort)(1547 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1025 * size), (ushort)(1538 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(514 * size), (ushort)(1 * size), (ushort)(512 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(512 * size), (ushort)(1025 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(523 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_K(byte index)
        {
            triangles.Add(new Triangle((ushort)(513 * size), (ushort)(524 * size), (ushort)(11 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(0 * size), (ushort)(513 * size), (ushort)(11 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1537 * size), (ushort)(1542 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1024 * size), (ushort)(1537 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1544 * size), (ushort)(1548 * size), (ushort)(1035 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1031 * size), (ushort)(1544 * size), (ushort)(1035 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1029 * size), (ushort)(1542 * size), (ushort)(1031 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(518 * size), (ushort)(1029 * size), (ushort)(1031 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_L(byte index)
        {
            triangles.Add(new Triangle((ushort)(524 * size), (ushort)(11 * size), (ushort)(513 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(0 * size), (ushort)(513 * size), (ushort)(11 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1536 * size), (ushort)(514 * size), (ushort)(1538 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(514 * size), (ushort)(1027 * size), (ushort)(1538 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_M(byte index)
        {
            triangles.Add(new Triangle((ushort)(524 * size), (ushort)(11 * size), (ushort)(513 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(0 * size), (ushort)(513 * size), (ushort)(11 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1547 * size), (ushort)(1034 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(1036 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(521 * size), (ushort)(523 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1024 * size), (ushort)(1028 * size), (ushort)(1541 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1541 * size), (ushort)(1537 * size), (ushort)(1024 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1291 * size), (ushort)(1547 * size), (ushort)(1541 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(778 * size), (ushort)(1034 * size), (ushort)(1031 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(774 * size), (ushort)(778 * size), (ushort)(1031 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1284 * size), (ushort)(1291 * size), (ushort)(1541 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_N(byte index)
        {
            triangles.Add(new Triangle((ushort)(524 * size), (ushort)(11 * size), (ushort)(513 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(0 * size), (ushort)(513 * size), (ushort)(11 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1547 * size), (ushort)(1034 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(1036 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(1547 * size), (ushort)(1537 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1024 * size), (ushort)(1034 * size), (ushort)(1537 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_O(byte index)
        {
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(10 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(1547 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1 * size), (ushort)(514 * size), (ushort)(10 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(1034 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1538 * size), (ushort)(1547 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1025 * size), (ushort)(1538 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(514 * size), (ushort)(1 * size), (ushort)(512 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(512 * size), (ushort)(1025 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_P(byte index)
        {
            triangles.Add(new Triangle((ushort)(524 * size), (ushort)(11 * size), (ushort)(513 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1542 * size), (ushort)(1547 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(1547 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(0 * size), (ushort)(513 * size), (ushort)(11 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1029 * size), (ushort)(1542 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(1034 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1029 * size), (ushort)(518 * size), (ushort)(1031 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_Q(byte index)
        {
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(10 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(1547 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1 * size), (ushort)(514 * size), (ushort)(10 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(1034 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1538 * size), (ushort)(1547 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1025 * size), (ushort)(1538 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(514 * size), (ushort)(1 * size), (ushort)(512 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(512 * size), (ushort)(1025 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1280 * size), (ushort)(1537 * size), (ushort)(1026 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(769 * size), (ushort)(1280 * size), (ushort)(1026 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_R(byte index)
        {
            triangles.Add(new Triangle((ushort)(524 * size), (ushort)(11 * size), (ushort)(513 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1031 * size), (ushort)(1544 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(1547 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(0 * size), (ushort)(513 * size), (ushort)(11 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1544 * size), (ushort)(1547 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(1034 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1029 * size), (ushort)(518 * size), (ushort)(1031 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1537 * size), (ushort)(1542 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1024 * size), (ushort)(1537 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1031 * size), (ushort)(1542 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_S(byte index)
        {
            triangles.Add(new Triangle((ushort)(1 * size), (ushort)(514 * size), (ushort)(3 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(1547 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(516 * size), (ushort)(3 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(1034 * size), (ushort)(1036 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1545 * size), (ushort)(1547 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1032 * size), (ushort)(1545 * size), (ushort)(1034 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(514 * size), (ushort)(1 * size), (ushort)(512 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(512 * size), (ushort)(1025 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(518 * size), (ushort)(1029 * size), (ushort)(1031 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1538 * size), (ushort)(1542 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1025 * size), (ushort)(1538 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1542 * size), (ushort)(1031 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(10 * size), (ushort)(518 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(7 * size), (ushort)(518 * size), (ushort)(10 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_T(byte index)
        {
            triangles.Add(new Triangle((ushort)(1036 * size), (ushort)(523 * size), (ushort)(1025 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(512 * size), (ushort)(1025 * size), (ushort)(523 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1034 * size), (ushort)(1036 * size), (ushort)(1547 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1547 * size), (ushort)(1034 * size), (ushort)(1545 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(523 * size), (ushort)(10 * size), (ushort)(521 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(10 * size), (ushort)(8 * size), (ushort)(521 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_U(byte index)
        {
            triangles.Add(new Triangle((ushort)(524 * size), (ushort)(11 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1 * size), (ushort)(514 * size), (ushort)(11 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1538 * size), (ushort)(1548 * size), (ushort)(1035 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1025 * size), (ushort)(1538 * size), (ushort)(1035 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(514 * size), (ushort)(1 * size), (ushort)(512 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(512 * size), (ushort)(1025 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_V(byte index)
        {
            triangles.Add(new Triangle((ushort)(524 * size), (ushort)(11 * size), (ushort)(514 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(4 * size), (ushort)(514 * size), (ushort)(11 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1537 * size), (ushort)(1548 * size), (ushort)(1035 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1024 * size), (ushort)(1537 * size), (ushort)(1035 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(514 * size), (ushort)(1024 * size), (ushort)(1025 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(515 * size), (ushort)(514 * size), (ushort)(1025 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_W(byte index)
        {
            triangles.Add(new Triangle((ushort)(524 * size), (ushort)(11 * size), (ushort)(513 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(2 * size), (ushort)(513 * size), (ushort)(11 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1545 * size), (ushort)(1548 * size), (ushort)(1035 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1032 * size), (ushort)(1545 * size), (ushort)(1035 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(513 * size), (ushort)(1024 * size), (ushort)(1026 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(515 * size), (ushort)(513 * size), (ushort)(1026 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1537 * size), (ushort)(1539 * size), (ushort)(1026 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1024 * size), (ushort)(1537 * size), (ushort)(1026 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1539 * size), (ushort)(1545 * size), (ushort)(1289 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1282 * size), (ushort)(1539 * size), (ushort)(1289 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1026 * size), (ushort)(1030 * size), (ushort)(773 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(770 * size), (ushort)(1026 * size), (ushort)(773 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_X(byte index)
        {
            triangles.Add(new Triangle((ushort)(524 * size), (ushort)(11 * size), (ushort)(518 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(8 * size), (ushort)(518 * size), (ushort)(11 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1544 * size), (ushort)(1548 * size), (ushort)(1035 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1030 * size), (ushort)(1544 * size), (ushort)(1035 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(518 * size), (ushort)(1028 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(519 * size), (ushort)(518 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(517 * size), (ushort)(3 * size), (ushort)(513 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(0 * size), (ushort)(513 * size), (ushort)(3 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1537 * size), (ushort)(1539 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1024 * size), (ushort)(1537 * size), (ushort)(1029 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(517 * size), (ushort)(1031 * size), (ushort)(1030 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(516 * size), (ushort)(517 * size), (ushort)(1030 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_Y(byte index)
        {
            triangles.Add(new Triangle((ushort)(1031 * size), (ushort)(1035 * size), (ushort)(1544 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(7 * size), (ushort)(518 * size), (ushort)(524 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(11 * size), (ushort)(7 * size), (ushort)(524 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1035 * size), (ushort)(1544 * size), (ushort)(1548 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1030 * size), (ushort)(519 * size), (ushort)(1025 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(512 * size), (ushort)(1025 * size), (ushort)(519 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1031 * size), (ushort)(1544 * size), (ushort)(1030 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }

        private void Add_Z(byte index)
        {
            triangles.Add(new Triangle((ushort)(1036 * size), (ushort)(10 * size), (ushort)(1547 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(10 * size), (ushort)(8 * size), (ushort)(1547 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1547 * size), (ushort)(1034 * size), (ushort)(1545 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1 * size), (ushort)(514 * size), (ushort)(3 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(3 * size), (ushort)(514 * size), (ushort)(516 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1545 * size), (ushort)(1034 * size), (ushort)(1032 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(516 * size), (ushort)(1545 * size), (ushort)(1032 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(512 * size), (ushort)(1538 * size), (ushort)(1 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(1538 * size), (ushort)(1540 * size), (ushort)(1 * size), color: 255) + (ushort)(index * size * 7 << 8));
            triangles.Add(new Triangle((ushort)(3 * size), (ushort)(516 * size), (ushort)(1032 * size), color: 255) + (ushort)(index * size * 7 << 8));

        }
    }
}