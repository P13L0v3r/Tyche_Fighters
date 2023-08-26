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
            this.position = position;
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

        public Text(string text, byte size = 1)
        {
            this.text = text;
            this.size = size;
        }

        public List<Triangle> Draw()
        {
            throw new NotImplementedException();
        }
    }
}