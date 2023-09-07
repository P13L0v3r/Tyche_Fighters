using OpenTK.Windowing.GraphicsLibraryFramework;
using static TycheFighters.Program;

namespace TycheFighters
{
    public class Boxer : Fighter
    {
        public override byte damage { get; protected set; } = 0;
        public override byte weight { get; init; } = 1;

        public override byte width { get; init; } = 32;
        public override byte height { get; init; } = 32;

        protected override Keys[] P1map { get; init; } = { Keys.W, Keys.A, Keys.D };
        protected override Keys[] P2map { get; init; } = { Keys.Up, Keys.Left, Keys.Right };

        private byte speed = 5;
        private byte jumpSpeed = 3;
        public Boxer(byte id, byte startX) : base(id, startX)
        {
            this.hitBox = new Collider
            (0,
                new Triangle[]
                {
                    new Triangle(PackWorldCoords(24,0), PackWorldCoords(24,16), PackWorldCoords(8,16)),
                    new Triangle(PackWorldCoords(24,0), PackWorldCoords(8,16), PackWorldCoords(8,0))
                }
            )
            {
                owner = this
            };
            this.attackBox.owner = this;

            this.animation = this.idle;
        }

        private Animation idle = 
            new Animation
            (
                new (byte, Frame)[]
                {
                    (0, new Frame(
                        new List<Triangle>{
                            // Box
                            new Triangle(PackWorldCoords(24,0), PackWorldCoords(24,16), PackWorldCoords(8,16), 192),
                            new Triangle(PackWorldCoords(24,0), PackWorldCoords(8,16), PackWorldCoords(8,0), 192),

                            // Flap
                            new Triangle(PackWorldCoords(25,12), PackWorldCoords(24,16), PackWorldCoords(16,11)),
                            new Triangle(PackWorldCoords(24,16), PackWorldCoords(8,16), PackWorldCoords(16,11)),
                            new Triangle(PackWorldCoords(16,11), PackWorldCoords(8,16), PackWorldCoords(7,10)),

                            // Tape
                            new Triangle(PackWorldCoords(14,0), PackWorldCoords(14,4), PackWorldCoords(18, 4), 100),
                            new Triangle(PackWorldCoords(14,0), PackWorldCoords(18,0), PackWorldCoords(18, 4), 100),
                        }
                    )),
                    (8, new Frame(
                        new List<Triangle>{
                            // Box
                            new Triangle(PackWorldCoords(25,0), PackWorldCoords(25,15), PackWorldCoords(7,15), 192),
                            new Triangle(PackWorldCoords(25,0), PackWorldCoords(7,15), PackWorldCoords(7,0), 192),

                            // Flap
                            new Triangle(PackWorldCoords(27,11), PackWorldCoords(25,15), PackWorldCoords(16,10)),
                            new Triangle(PackWorldCoords(25,15), PackWorldCoords(7,15), PackWorldCoords(16,10)),
                            new Triangle(PackWorldCoords(16,10), PackWorldCoords(7,15), PackWorldCoords(5,9)),

                            // Tape
                            new Triangle(PackWorldCoords(14,0), PackWorldCoords(14,3), PackWorldCoords(18, 3), 100),
                            new Triangle(PackWorldCoords(14,0), PackWorldCoords(18,0), PackWorldCoords(18, 3), 100),
                        }
                    )),
                    (15, new Frame(new List<Triangle>{})),
                }, true
            );
        private Animation close = 
            new Animation
            (
                new (byte, Frame)[]
                {
                    (0, new Frame(
                        new List<Triangle>{
                            // Box
                            new Triangle(PackWorldCoords(24,0), PackWorldCoords(24,16), PackWorldCoords(8,16), 192),
                            new Triangle(PackWorldCoords(24,0), PackWorldCoords(8,16), PackWorldCoords(8,0), 192),

                            // Flap       
                            new Triangle(PackWorldCoords(25,12), PackWorldCoords(24,16), PackWorldCoords(16,11)),
                            new Triangle(PackWorldCoords(24,16), PackWorldCoords(8,16), PackWorldCoords(16,11)),
                            new Triangle(PackWorldCoords(16,11), PackWorldCoords(8,16), PackWorldCoords(7,10)),
                            
                            // Tape
                            new Triangle(PackWorldCoords(14,0), PackWorldCoords(14,4), PackWorldCoords(18, 4), 100),
                            new Triangle(PackWorldCoords(14,0), PackWorldCoords(18,0), PackWorldCoords(18, 4), 100),
                        }
                    )),
                    (1, new Frame(
                        new List<Triangle>{
                            // Box
                            new Triangle(PackWorldCoords(24,0), PackWorldCoords(24,16), PackWorldCoords(8,16), 192),
                            new Triangle(PackWorldCoords(24,0), PackWorldCoords(8,16), PackWorldCoords(8,0), 192),

                            // Flap       
                            new Triangle(PackWorldCoords(26,14), PackWorldCoords(24,16), PackWorldCoords(16,14)),
                            new Triangle(PackWorldCoords(24,16), PackWorldCoords(8,16), PackWorldCoords(16,14)),
                            new Triangle(PackWorldCoords(16,14), PackWorldCoords(8,16), PackWorldCoords(6,14)),
                            
                            // Tape
                            new Triangle(PackWorldCoords(14,0), PackWorldCoords(14,4), PackWorldCoords(18, 4), 100),
                            new Triangle(PackWorldCoords(14,0), PackWorldCoords(18,0), PackWorldCoords(18, 4), 100),
                            
                            // Flap       
                            new Triangle(PackWorldCoords(26,18), PackWorldCoords(24,16), PackWorldCoords(16,18), 128),
                            new Triangle(PackWorldCoords(24,16), PackWorldCoords(8,16), PackWorldCoords(16,18), 128),
                            new Triangle(PackWorldCoords(16,18), PackWorldCoords(8,16), PackWorldCoords(6,18), 128),
                            
                            // Tape
                            new Triangle(PackWorldCoords(14,12), PackWorldCoords(14,16), PackWorldCoords(18, 16), 100),
                            new Triangle(PackWorldCoords(14,12), PackWorldCoords(18,12), PackWorldCoords(18, 16), 100),

                            new Triangle(PackWorldCoords(14,16), PackWorldCoords(13,18), PackWorldCoords(19, 18), 64),
                            new Triangle(PackWorldCoords(14,16), PackWorldCoords(18,16), PackWorldCoords(19, 18), 64),
                        }
                    )),
                    (2, new Frame(
                        new List<Triangle>{
                            // Box
                            new Triangle(PackWorldCoords(24,0), PackWorldCoords(24,16), PackWorldCoords(8,16), 192),
                            new Triangle(PackWorldCoords(24,0), PackWorldCoords(8,16), PackWorldCoords(8,0), 192),

                            // Flap       
                            new Triangle(PackWorldCoords(22,18), PackWorldCoords(24,16), PackWorldCoords(16,18)),
                            new Triangle(PackWorldCoords(24,16), PackWorldCoords(8,16), PackWorldCoords(16,18)),
                            new Triangle(PackWorldCoords(16,18), PackWorldCoords(8,16), PackWorldCoords(10,18)),

                            // Flap       
                            new Triangle(PackWorldCoords(24,16), PackWorldCoords(8,16), PackWorldCoords(8,24), 128),
                            new Triangle(PackWorldCoords(24,16), PackWorldCoords(8,24), PackWorldCoords(24,24), 128),
                            
                            // Tape
                            new Triangle(PackWorldCoords(14,0), PackWorldCoords(14,4), PackWorldCoords(18, 4), 100),
                            new Triangle(PackWorldCoords(14,0), PackWorldCoords(18,0), PackWorldCoords(18, 4), 100),

                            new Triangle(PackWorldCoords(14,12), PackWorldCoords(14,16), PackWorldCoords(18, 16), 100),
                            new Triangle(PackWorldCoords(14,12), PackWorldCoords(18,12), PackWorldCoords(18, 16), 100),

                            new Triangle(PackWorldCoords(14,16), PackWorldCoords(14,24), PackWorldCoords(18, 24), 64),
                            new Triangle(PackWorldCoords(14,16), PackWorldCoords(18,16), PackWorldCoords(18, 24), 64),
                            
                            // Tape
                            new Triangle(PackWorldCoords(14,16), PackWorldCoords(15,18), PackWorldCoords(17, 18), 128),
                            new Triangle(PackWorldCoords(14,16), PackWorldCoords(18,16), PackWorldCoords(17, 18), 128),
                        }
                    )),
                    (3, new Frame(
                        new List<Triangle>{
                            // Box
                            new Triangle(PackWorldCoords(24,0), PackWorldCoords(24,16), PackWorldCoords(8,16), 192),
                            new Triangle(PackWorldCoords(24,0), PackWorldCoords(8,16), PackWorldCoords(8,0), 192),
                            
                            // Tape
                            new Triangle(PackWorldCoords(14,0), PackWorldCoords(14,4), PackWorldCoords(18, 4), 100),
                            new Triangle(PackWorldCoords(14,0), PackWorldCoords(18,0), PackWorldCoords(18, 4), 100),

                            new Triangle(PackWorldCoords(14,12), PackWorldCoords(14,16), PackWorldCoords(18, 16), 100),
                            new Triangle(PackWorldCoords(14,12), PackWorldCoords(18,12), PackWorldCoords(18, 16), 100),
                        }
                    )),
                }, false
            );

        private Animation spin = 
            new Animation(
                new (byte, Frame)[]
                {
                    (0, new Frame(
                        new List<Triangle>{
                            // Box
                            new Triangle(PackWorldCoords(5,12),PackWorldCoords(22,2),PackWorldCoords(22,22), 64),
                            new Triangle(PackWorldCoords(22,22),PackWorldCoords(16,23),PackWorldCoords(10,22), 64),
                            new Triangle(PackWorldCoords(10,22),PackWorldCoords(6,18),PackWorldCoords(5,12), 64),
                            new Triangle(PackWorldCoords(5,12),PackWorldCoords(6,6),PackWorldCoords(10,2), 64),
                            new Triangle(PackWorldCoords(10,2),PackWorldCoords(16,1),PackWorldCoords(22,2), 64),
                            new Triangle(PackWorldCoords(22,2),PackWorldCoords(26,6),PackWorldCoords(27,12), 64),
                            new Triangle(PackWorldCoords(27,12),PackWorldCoords(26,18),PackWorldCoords(22,22), 64),
                            new Triangle(PackWorldCoords(22,22),PackWorldCoords(10,22),PackWorldCoords(5,12), 64),
                            new Triangle(PackWorldCoords(5,12),PackWorldCoords(10,2),PackWorldCoords(22,2), 64),
                            new Triangle(PackWorldCoords(22,2),PackWorldCoords(27,12),PackWorldCoords(22,22), 64),

                            new Triangle(PackWorldCoords(5,9),PackWorldCoords(19,1),PackWorldCoords(27,15), 128),
                            new Triangle(PackWorldCoords(13,23),PackWorldCoords(5,9),PackWorldCoords(27,15), 128),

                            new Triangle(PackWorldCoords(6,18),PackWorldCoords(10,2),PackWorldCoords(26,6), 192),
                            new Triangle(PackWorldCoords(22,22),PackWorldCoords(6,18),PackWorldCoords(26,6), 192),



                            
                            // Tape
                            
                            new Triangle(PackWorldCoords(8,12),PackWorldCoords(20,5),PackWorldCoords(20,19), 128),
                            new Triangle(PackWorldCoords(20,19),PackWorldCoords(16,20),PackWorldCoords(12,19), 128),
                            new Triangle(PackWorldCoords(12,19),PackWorldCoords(9,16),PackWorldCoords(8,12), 128),
                            new Triangle(PackWorldCoords(8,12),PackWorldCoords(9,8),PackWorldCoords(12,5), 128),
                            new Triangle(PackWorldCoords(12,5),PackWorldCoords(16,4),PackWorldCoords(20,5), 128),
                            new Triangle(PackWorldCoords(20,5),PackWorldCoords(23,8),PackWorldCoords(24,12), 128),
                            new Triangle(PackWorldCoords(24,12),PackWorldCoords(23,16),PackWorldCoords(20,19), 128),
                            new Triangle(PackWorldCoords(20,19),PackWorldCoords(12,19),PackWorldCoords(8,12), 128),
                            new Triangle(PackWorldCoords(8,12),PackWorldCoords(12,5),PackWorldCoords(20,5), 128),
                            new Triangle(PackWorldCoords(20,5),PackWorldCoords(24,12),PackWorldCoords(20,19), 128),

                            new Triangle(PackWorldCoords(12,12),PackWorldCoords(18,8),PackWorldCoords(18,16), 192),
                            new Triangle(PackWorldCoords(18,16),PackWorldCoords(16,16),PackWorldCoords(14,16), 192),
                            new Triangle(PackWorldCoords(14,16), PackWorldCoords(12,14), PackWorldCoords(12,12), 192),
                            new Triangle(PackWorldCoords(12,12),PackWorldCoords(12,10),PackWorldCoords(14,8), 192),
                            new Triangle(PackWorldCoords(14,8),PackWorldCoords(16,8),PackWorldCoords(18,8), 192),
                            new Triangle(PackWorldCoords(18,8),PackWorldCoords(20,10),PackWorldCoords(20,12), 192),
                            new Triangle(PackWorldCoords(20,12),PackWorldCoords(20,14),PackWorldCoords(18,16), 192),
                            new Triangle(PackWorldCoords(18,16),PackWorldCoords(14,16),PackWorldCoords(12,12), 192),
                            new Triangle(PackWorldCoords(12,12),PackWorldCoords(14,8),PackWorldCoords(18,8), 192),
                            new Triangle(PackWorldCoords(18,8),PackWorldCoords(20,12),PackWorldCoords(18,16), 192),

                            new Triangle(PackWorldCoords(14,4), PackWorldCoords(14,8), PackWorldCoords(18, 8), 100),
                            new Triangle(PackWorldCoords(14,4), PackWorldCoords(18,4), PackWorldCoords(18, 8), 100),

                            new Triangle(PackWorldCoords(14,16), PackWorldCoords(14,20), PackWorldCoords(18, 20), 100),
                            new Triangle(PackWorldCoords(14,16), PackWorldCoords(18,16), PackWorldCoords(18, 20), 100),
                        }
                    )),
                    (1, new Frame(
                        new List<Triangle>{
                            // Box
                            new Triangle(PackWorldCoords(5,12),PackWorldCoords(22,2),PackWorldCoords(22,22), 64),
                            new Triangle(PackWorldCoords(22,22),PackWorldCoords(16,23),PackWorldCoords(10,22), 64),
                            new Triangle(PackWorldCoords(10,22),PackWorldCoords(6,18),PackWorldCoords(5,12), 64),
                            new Triangle(PackWorldCoords(5,12),PackWorldCoords(6,6),PackWorldCoords(10,2), 64),
                            new Triangle(PackWorldCoords(10,2),PackWorldCoords(16,1),PackWorldCoords(22,2), 64),
                            new Triangle(PackWorldCoords(22,2),PackWorldCoords(26,6),PackWorldCoords(27,12), 64),
                            new Triangle(PackWorldCoords(27,12),PackWorldCoords(26,18),PackWorldCoords(22,22), 64),
                            new Triangle(PackWorldCoords(22,22),PackWorldCoords(10,22),PackWorldCoords(5,12), 64),
                            new Triangle(PackWorldCoords(5,12),PackWorldCoords(10,2),PackWorldCoords(22,2), 64),
                            new Triangle(PackWorldCoords(22,2),PackWorldCoords(27,12),PackWorldCoords(22,22), 64),

                            new Triangle(PackWorldCoords(6,18),PackWorldCoords(10,2),PackWorldCoords(26,6), 128),
                            new Triangle(PackWorldCoords(22,22),PackWorldCoords(6,18),PackWorldCoords(26,6), 128),

                            new Triangle(PackWorldCoords(5,9),PackWorldCoords(19,1),PackWorldCoords(27,15), 192),
                            new Triangle(PackWorldCoords(13,23),PackWorldCoords(5,9),PackWorldCoords(27,15), 192),

                            
                            // Tape
                            
                            new Triangle(PackWorldCoords(8,12),PackWorldCoords(20,5),PackWorldCoords(20,19), 128),
                            new Triangle(PackWorldCoords(20,19),PackWorldCoords(16,20),PackWorldCoords(12,19), 128),
                            new Triangle(PackWorldCoords(12,19),PackWorldCoords(9,16),PackWorldCoords(8,12), 128),
                            new Triangle(PackWorldCoords(8,12),PackWorldCoords(9,8),PackWorldCoords(12,5), 128),
                            new Triangle(PackWorldCoords(12,5),PackWorldCoords(16,4),PackWorldCoords(20,5), 128),
                            new Triangle(PackWorldCoords(20,5),PackWorldCoords(23,8),PackWorldCoords(24,12), 128),
                            new Triangle(PackWorldCoords(24,12),PackWorldCoords(23,16),PackWorldCoords(20,19), 128),
                            new Triangle(PackWorldCoords(20,19),PackWorldCoords(12,19),PackWorldCoords(8,12), 128),
                            new Triangle(PackWorldCoords(8,12),PackWorldCoords(12,5),PackWorldCoords(20,5), 128),
                            new Triangle(PackWorldCoords(20,5),PackWorldCoords(24,12),PackWorldCoords(20,19), 128),

                            new Triangle(PackWorldCoords(12,12),PackWorldCoords(18,8),PackWorldCoords(18,16), 192),
                            new Triangle(PackWorldCoords(18,16),PackWorldCoords(16,16),PackWorldCoords(14,16), 192),
                            new Triangle(PackWorldCoords(14,16), PackWorldCoords(12,14), PackWorldCoords(12,12), 192),
                            new Triangle(PackWorldCoords(12,12),PackWorldCoords(12,10),PackWorldCoords(14,8), 192),
                            new Triangle(PackWorldCoords(14,8),PackWorldCoords(16,8),PackWorldCoords(18,8), 192),
                            new Triangle(PackWorldCoords(18,8),PackWorldCoords(20,10),PackWorldCoords(20,12), 192),
                            new Triangle(PackWorldCoords(20,12),PackWorldCoords(20,14),PackWorldCoords(18,16), 192),
                            new Triangle(PackWorldCoords(18,16),PackWorldCoords(14,16),PackWorldCoords(12,12), 192),
                            new Triangle(PackWorldCoords(12,12),PackWorldCoords(14,8),PackWorldCoords(18,8), 192),
                            new Triangle(PackWorldCoords(18,8),PackWorldCoords(20,12),PackWorldCoords(18,16), 192),

                            new Triangle(PackWorldCoords(24,10),PackWorldCoords(24,14),PackWorldCoords(20,14), 100),
                            new Triangle(PackWorldCoords(12,10),PackWorldCoords(12,14),PackWorldCoords(8,14), 100),
                            new Triangle(PackWorldCoords(8,10),PackWorldCoords(12,10),PackWorldCoords(8,14), 100),
                            new Triangle(PackWorldCoords(20,10),PackWorldCoords(24,10),PackWorldCoords(20,14), 100),

                        }
                    )),
                    (2, new Frame(
                        new List<Triangle>() { }
                    ))
                }, true
            );

        private Animation open =
            new Animation
            (
                new (byte, Frame)[]
                {
                    (0, new Frame(
                        new List<Triangle>{
                            // Box
                            new Triangle(PackWorldCoords(24,0), PackWorldCoords(24,16), PackWorldCoords(8,16), 192),
                            new Triangle(PackWorldCoords(24,0), PackWorldCoords(8,16), PackWorldCoords(8,0), 192),
                            
                            // Tape
                            new Triangle(PackWorldCoords(14,0), PackWorldCoords(14,4), PackWorldCoords(18, 4), 100),
                            new Triangle(PackWorldCoords(14,0), PackWorldCoords(18,0), PackWorldCoords(18, 4), 100),

                            new Triangle(PackWorldCoords(14,12), PackWorldCoords(14,16), PackWorldCoords(18, 16), 100),
                            new Triangle(PackWorldCoords(14,12), PackWorldCoords(18,12), PackWorldCoords(18, 16), 100),
                        }
                    )),
                    (1, new Frame(
                        new List<Triangle>{
                            // Box
                            new Triangle(PackWorldCoords(24,0), PackWorldCoords(24,16), PackWorldCoords(8,16), 192),
                            new Triangle(PackWorldCoords(24,0), PackWorldCoords(8,16), PackWorldCoords(8,0), 192),

                            // Flap       
                            new Triangle(PackWorldCoords(22,18), PackWorldCoords(24,16), PackWorldCoords(16,18)),
                            new Triangle(PackWorldCoords(24,16), PackWorldCoords(8,16), PackWorldCoords(16,18)),
                            new Triangle(PackWorldCoords(16,18), PackWorldCoords(8,16), PackWorldCoords(10,18)),

                            // Flap       
                            new Triangle(PackWorldCoords(24,16), PackWorldCoords(8,16), PackWorldCoords(8,24), 128),
                            new Triangle(PackWorldCoords(24,16), PackWorldCoords(8,24), PackWorldCoords(24,24), 128),
                            
                            // Tape
                            new Triangle(PackWorldCoords(14,0), PackWorldCoords(14,4), PackWorldCoords(18, 4), 100),
                            new Triangle(PackWorldCoords(14,0), PackWorldCoords(18,0), PackWorldCoords(18, 4), 100),

                            new Triangle(PackWorldCoords(14,12), PackWorldCoords(14,16), PackWorldCoords(18, 16), 100),
                            new Triangle(PackWorldCoords(14,12), PackWorldCoords(18,12), PackWorldCoords(18, 16), 100),

                            new Triangle(PackWorldCoords(14,16), PackWorldCoords(14,24), PackWorldCoords(18, 24), 64),
                            new Triangle(PackWorldCoords(14,16), PackWorldCoords(18,16), PackWorldCoords(18, 24), 64),
                            
                            // Tape
                            new Triangle(PackWorldCoords(14,16), PackWorldCoords(15,18), PackWorldCoords(17, 18), 128),
                            new Triangle(PackWorldCoords(14,16), PackWorldCoords(18,16), PackWorldCoords(17, 18), 128),
                        }
                    )),
                    (2, new Frame(
                        new List<Triangle>{
                            // Box
                            new Triangle(PackWorldCoords(24,0), PackWorldCoords(24,16), PackWorldCoords(8,16), 192),
                            new Triangle(PackWorldCoords(24,0), PackWorldCoords(8,16), PackWorldCoords(8,0), 192),

                            // Flap       
                            new Triangle(PackWorldCoords(26,14), PackWorldCoords(24,16), PackWorldCoords(16,14)),
                            new Triangle(PackWorldCoords(24,16), PackWorldCoords(8,16), PackWorldCoords(16,14)),
                            new Triangle(PackWorldCoords(16,14), PackWorldCoords(8,16), PackWorldCoords(6,14)),
                            
                            // Tape
                            new Triangle(PackWorldCoords(14,0), PackWorldCoords(14,4), PackWorldCoords(18, 4), 100),
                            new Triangle(PackWorldCoords(14,0), PackWorldCoords(18,0), PackWorldCoords(18, 4), 100),
                            
                            // Flap       
                            new Triangle(PackWorldCoords(26,18), PackWorldCoords(24,16), PackWorldCoords(16,18), 128),
                            new Triangle(PackWorldCoords(24,16), PackWorldCoords(8,16), PackWorldCoords(16,18), 128),
                            new Triangle(PackWorldCoords(16,18), PackWorldCoords(8,16), PackWorldCoords(6,18), 128),
                            
                            // Tape
                            new Triangle(PackWorldCoords(14,12), PackWorldCoords(14,16), PackWorldCoords(18, 16), 100),
                            new Triangle(PackWorldCoords(14,12), PackWorldCoords(18,12), PackWorldCoords(18, 16), 100),

                            new Triangle(PackWorldCoords(14,16), PackWorldCoords(13,18), PackWorldCoords(19, 18), 64),
                            new Triangle(PackWorldCoords(14,16), PackWorldCoords(18,16), PackWorldCoords(19, 18), 64),
                        }
                    )),
                    (3, new Frame(
                        new List<Triangle>{
                            // Box
                            new Triangle(PackWorldCoords(24,0), PackWorldCoords(24,16), PackWorldCoords(8,16), 192),
                            new Triangle(PackWorldCoords(24,0), PackWorldCoords(8,16), PackWorldCoords(8,0), 192),

                            // Flap       
                            new Triangle(PackWorldCoords(25,12), PackWorldCoords(24,16), PackWorldCoords(16,11)),
                            new Triangle(PackWorldCoords(24,16), PackWorldCoords(8,16), PackWorldCoords(16,11)),
                            new Triangle(PackWorldCoords(16,11), PackWorldCoords(8,16), PackWorldCoords(7,10)),
                            
                            // Tape
                            new Triangle(PackWorldCoords(14,0), PackWorldCoords(14,4), PackWorldCoords(18, 4), 100),
                            new Triangle(PackWorldCoords(14,0), PackWorldCoords(18,0), PackWorldCoords(18, 4), 100),
                        }
                    )),                    
                }, false
            );

        private Animation KO = new Animation
        (
            new (byte, Frame)[]
            {
                (0, new Frame(
                        new List<Triangle>{
                            // Box
                            new Triangle(PackWorldCoords(24,0), PackWorldCoords(24,16), PackWorldCoords(8,16), 192),
                            new Triangle(PackWorldCoords(24,0), PackWorldCoords(8,16), PackWorldCoords(8,0), 192),
                            
                            // Tape
                            new Triangle(PackWorldCoords(24,6),PackWorldCoords(24,10),PackWorldCoords(20,10), 100),
                            new Triangle(PackWorldCoords(12,6),PackWorldCoords(12,10),PackWorldCoords(8,10), 100),
                            new Triangle(PackWorldCoords(8,6),PackWorldCoords(12,6),PackWorldCoords(8,10), 100),
                            new Triangle(PackWorldCoords(20,6),PackWorldCoords(24,6),PackWorldCoords(20,10), 100),
                        }
                    ))
            }
        );

        private Collider attackBox = new Collider
                    (1,
                        new Triangle[]
                        {
                            new Triangle(PackWorldCoords(5,12),PackWorldCoords(22,2),PackWorldCoords(22,22)),
                            new Triangle(PackWorldCoords(22,22),PackWorldCoords(16,23),PackWorldCoords(10,22)),
                            new Triangle(PackWorldCoords(10,22),PackWorldCoords(6,18),PackWorldCoords(5,12)),
                            new Triangle(PackWorldCoords(5,12),PackWorldCoords(6,6),PackWorldCoords(10,2)),
                            new Triangle(PackWorldCoords(10,2),PackWorldCoords(16,1),PackWorldCoords(22,2)),
                            new Triangle(PackWorldCoords(22,2),PackWorldCoords(26,6),PackWorldCoords(27,12)),
                            new Triangle(PackWorldCoords(27,12),PackWorldCoords(26,18),PackWorldCoords(22,22)),
                            new Triangle(PackWorldCoords(22,22),PackWorldCoords(10,22),PackWorldCoords(5,12)),
                            new Triangle(PackWorldCoords(5,12),PackWorldCoords(10,2),PackWorldCoords(22,2)),
                            new Triangle(PackWorldCoords(22,2),PackWorldCoords(27,12),PackWorldCoords(22,22)),
                        }
                    );

        public Animation animation;

        public override void Update()
        {
            base.Update();

            if (knockedOut)
            {
                animation = KO;
                animation.Start();
                Fall();
                currentFrame = animation.CurrentFrame();
                animation.NextFrame();
            }

            if (animation == close && animation.Finished())
            {
                jumpFrame = 1;
                fallFrame = 0;

                animation = spin;
                animation.Start();
            }

            if ( (byte)position < 3 * fallFrame + STAGE_FLOOR_HEIGHT + 3 && animation == spin && jumpFrame == 0)
            {
                animation = open;
                animation.Start();
            }

            if (animation == open && animation.Finished())
            {
                animation = idle;
                animation.Start();
            }

            Jump();

            Fall();

            currentFrame = animation.CurrentFrame();
            animation.NextFrame();

            hitBox.origin = position;

            colliders = new List<Collider>() { hitBox };

            if (animation == spin)
            {
                hitBox.origin = (ushort)(position + PackWorldCoords(0, 4));

                attackBox.attack = attackMult;
                attackBox.origin = position;

                colliders.Add(attackBox);
            }
        }

        private byte attackMult = 0;

        public override void ReadKeyboard(KeyboardState keyState)
        {
            if (keyState.IsKeyPressed(id == 0 ? P1map[0] : P2map[0]))
            {
                //Console.WriteLine(localRandom.Next(256));
                if (jumpFrame == 0 && fallFrame == 0 && animation != close)
                {
                    animation = close;
                    animation.Start();
                }
                else if (fallFrame > 0 && animation == spin)
                {
                    if (localRandom.Next(256) > attackMult)
                    {
                        attackMult += fallFrame;
                    }
                    jumpFrame = 1;
                    fallFrame = 0;

                    //Console.WriteLine(attackMult);
                }
            }

            if (keyState.IsKeyDown(id == 0 ? P1map[1] : P2map[1]))
            {
                flipped = true;

                if (fallFrame > 0)
                {
                    if ((byte)(position >> 8) >= jumpSpeed)
                    {
                        position -= PackWorldCoords(jumpSpeed, 0);
                    }
                    else
                    {
                        position = PackWorldCoords(0, (byte)position);
                    }
                }
                else
                {
                    if ((byte)(position >> 8) >= speed)
                    {
                        position -= PackWorldCoords(speed, 0);
                    }
                    else
                    {
                        position = PackWorldCoords(0, (byte)position);
                    }
                }
            }
        

            if (keyState.IsKeyDown(id == 0 ? P1map[2] : P2map[2]))
            {
                flipped = false;
                
                if (fallFrame > 0)
                {
                    if ((byte)(position >> 8) + width + jumpSpeed <= 255)
                    {
                        position += PackWorldCoords(jumpSpeed, 0);
                    }
                    else
                    {
                        position = PackWorldCoords((byte)(255 - width), (byte)position);
                    }
                }
                else
                {
                    if ((byte)(position >> 8) + width + speed <= 255)
                    {
                        position += PackWorldCoords(speed, 0);
                    }
                    else
                    {
                        position = PackWorldCoords((byte)(255 - width), (byte)position);
                    }
                }
            }
        }

        public override void Collision(Collider internalCollider, Collider externalCollider)
        {
            if (internalCollider.id == 0 && externalCollider.attack > 0)
            {
                TakeDamage(externalCollider.attack);
            }
            else if (externalCollider.id == 0 && internalCollider.attack > 0)
            {
                animation = open;
                animation.Start();
            }
        }

        private byte jumpFrame = 0;
        private ushort initJumpPos;
        private void Jump()
        {
            if (jumpFrame == 0)
            {
                initJumpPos = position;
            }
            else if (jumpFrame > 0 && jumpFrame <= 6)
            {
                if ((byte)position + height + 5 <= 255)
                {
                    position += PackWorldCoords(0, 5);
                }
                else
                {
                    position = PackWorldCoords((byte)(position >> 8), (byte)(255 - height));
                }
                
                jumpFrame++;
            }
            else if (jumpFrame > 6 && jumpFrame <= 11)
            {
                if ((byte)position + height + 1 <= 255)
                {
                    position += PackWorldCoords(0, 1);
                }
                else
                {
                    position = PackWorldCoords((byte)(position >> 8), (byte)(255 - height));
                }

                jumpFrame++;
            }
            else
            {
                jumpFrame = 0;
                fallFrame = 1;
            }
        }

        private ushort initFallPos;

        private void Fall()
        {
            if (fallFrame == 0)
            {
                initFallPos = position;
            }
            else
            {
                position -= PackWorldCoords(0, fallFrame);
                if ((position & 0b11111111) <= STAGE_FLOOR_HEIGHT)
                {
                    position = PackWorldCoords((byte)(position >> 8), STAGE_FLOOR_HEIGHT);
                    fallFrame = 0;
                    attackMult = 0;
                }
                else
                {
                    fallFrame++;
                }
            }
        }
    }
}