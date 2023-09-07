using OpenTK.Windowing.GraphicsLibraryFramework;
using static TycheFighters.Program;

namespace TycheFighters
{
    public class Dummy : Fighter
    {
        public Dummy(byte id, byte startX) : base(id, startX)
        {
            this.hitBox = new Collider
            (0,
                new Triangle[]
                {
                    new Triangle(PackWorldCoords(24,0), PackWorldCoords(24,32), PackWorldCoords(8,32)),
                    new Triangle(PackWorldCoords(24,0), PackWorldCoords(8,32), PackWorldCoords(8,0))
                }
            );
        }

        public override byte damage { get; protected set; } = 0;
        public override byte weight { get; init; } = 5;

        public override byte width { get; init; } = 32;
        public override byte height { get; init; } = 32;

        protected override Keys[] P1map { get; init; } = { };
        protected override Keys[] P2map { get; init; } = { };

        public override void ReadKeyboard(KeyboardState keyState)
        {
            
        }

        public override void Collision(Collider internalCollider, Collider externalCollider)
        {
            if (internalCollider.id == 0 && externalCollider.attack > 0)
            {
                TakeDamage(externalCollider.attack);
            }
        }

        public override void Update()
        {
            currentFrame = new Frame
            (
                new List<Triangle>()
                {
                    new Triangle(3584,4608,4615,color:200),
                    new Triangle(3591,3584,4615,color:200),
                    new Triangle(3351,4887,3591,color:200),
                    new Triangle(4887,3351,5146,color:200),
                    new Triangle(3098,5146,3351,color:200),
                    new Triangle(3591,4615,4887,color:200),
                    new Triangle(3351,3591,2068,color:200),
                    new Triangle(2568,2068,3591,color:200),
                    new Triangle(4887,4615,6164,color:200),
                    new Triangle(6164,5654,4887,color:200),
                    new Triangle(5640,6164,4615,color:200),
                    new Triangle(2068,2582,3351,color:200),
                    new Triangle(5146,3098,5149,color:200),
                    new Triangle(3101,5149,3098,color:200),
                    new Triangle(4638,5149,3101,color:200),
                    new Triangle(3614,4638,3101,color:200),
                }
            );

            hitBox.origin = position;

            colliders = new List<Collider>() { hitBox };
        }
    }
}