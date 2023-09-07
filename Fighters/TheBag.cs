using OpenTK.Windowing.GraphicsLibraryFramework;
using static TycheFighters.Program;

namespace TycheFighters
{
    public class TheBag : Fighter
    {
        public Animation animation;
        public TheBag(byte id, byte startX) : base(id, startX)
        {
            hitBox = new Collider
            (0,
                new Triangle[]
                {
                    new Triangle(2816,8192,8237),
                    new Triangle(2861, 2816, 8237)
                }
            )
            {
                owner = this
            };
            this.dashAttack.owner = this;
            this.animation = this.idle;

            this.allComboes = new Combo[]{ 0b010_010, 0b010_011, 0b010_001, 0b100_110_100_110, 0b100_100, 0b110_110 };
            this.comboResults = new Action[]
            {
                () => { if ((jumpFrame | fallFrame | dashFrame) == 0) jumpFrame = 1; },
                () => { if (dashFrame == 0) { jumpFrame = 1; dashFrame = 16; flipped = false; dashAttack.ClearFlag(); }},
                () => { if (dashFrame == 0) { jumpFrame = 1; dashFrame = 16; flipped = true; dashAttack.ClearFlag(); }},
                () => { sweepFrame = 6; dashAttack.ClearFlag(); },
                () => { if (dashFrame == 0) { dashFrame = 8; flipped = true; dashAttack.ClearFlag(); }},
                () => { if (dashFrame == 0) { dashFrame = 8; flipped = false; dashAttack.ClearFlag(); } },
            };
        }

        private Collider dashAttack = new Collider(1, new Triangle[] { new Triangle(2565, 14085, 14105), new Triangle(2585, 2565, 14105) }, 4);

        public override byte damage { get; protected set; }
        public override byte weight { get; init; } = 3;
        public override byte width { get; init; } = 64;
        public override byte height { get; init; } = 64;
        protected override Keys[] P1map { get; init; } = { Keys.D1, Keys.D2, Keys.D3, Keys.Q, Keys.W, Keys.E };
        protected override Keys[] P2map { get; init; } = { Keys.K, Keys.L, Keys.Semicolon, Keys.Comma, Keys.Period, Keys.Slash };

        public override void Collision(Collider internalCollider, Collider externalCollider)
        {
            if (externalCollider.id == 0 && internalCollider.attack > 0)
            {
                if (dashFrame > 0)
                {
                    internalCollider.Flag();
                    externalCollider.owner.InitLaunch((sbyte)Math.Sign((byte)(externalCollider.owner.position >> 8) - (byte)(position >> 8)), (sbyte)Math.Sign((byte)externalCollider.owner.position - (byte)position));
                }
            }

            if (internalCollider.id == 0 && externalCollider.attack > 0)
            {
                TakeDamage(externalCollider.attack);
                jumpFrame = 1;
                fallFrame = 0;
            }
        }

        public override void ReadKeyboard(KeyboardState keyState)
        {
            for (byte i = 0; i < P1map.Length; i++)
            {
                if (keyState.IsKeyPressed(id == 0 ? P1map[i] : P2map[i]))
                {
                    AddKey((byte)(i + 1));
                }
            }

            CheckCombo();
        }

        public override void Update()
        {
            base.Update();

            if (knockedOut)
            {
                animation = dash;
                animation.Start();
                Fall();
                currentFrame = animation.CurrentFrame();
                animation.NextFrame();
                return;
            }

            if (jumpFrame > 0 || fallFrame > 0)
            {
                animation = jump;
                animation.Start();
            }

            if (jumpFrame == 0 && fallFrame == 0)
            {
                animation = idle;
                animation.Start();
            }

            if (dashFrame > 0)
            {
                if ((byte)position > STAGE_FLOOR_HEIGHT)
                {
                    animation = dash;
                    animation.Start();
                }
                else
                {
                    animation = sweep;
                    animation.Start();
                }
            }

            if (sweepFrame > 0)
            {
                animation = sweep;
                animation.Start();
            }

            Jump();
            Fall();
            Dash();
            Sweep();

            currentFrame = animation.CurrentFrame();
            animation.NextFrame();

            hitBox.origin = flipped ? (ushort)(position + PackWorldCoords(21, 0)) : position;
            dashAttack.origin = position;
            dashAttack.attack = dashAttack.IsFlagged() ? (byte)0 : animation == sweep ? sweepFrame > 0 ? (byte)4 : (byte)16 : (byte)8;

            colliders = new List<Collider>() { (dashFrame | sweepFrame) > 0 ? dashAttack : hitBox };
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
                }
                else
                {
                    fallFrame++;
                }
            }
        }

        private byte dashFrame = 0;
        private ushort initDashPos;
        private void Dash()
        {
            if (dashFrame == 0)
            {
                initDashPos = position;
            }
            else
            {
                if (flipped)
                {
                    position -= PackWorldCoords(Math.Min(dashFrame, (byte)(position >> 8)), 0);
                    if ((byte)(position >> 8) == 0) { flipped = false; dashAttack.ClearFlag(); }
                }
                else
                {
                    position += PackWorldCoords(Math.Min(dashFrame, (byte)(255 - (position >> 8) - width)), 0);
                    if ((byte)(position >> 8) == 255 - width) { flipped = true; dashAttack.ClearFlag(); }
                }

                dashFrame = Math.Max((byte)(dashFrame - 1), (byte)0);
            }
        }

        private byte sweepFrame = 0;
        private void Sweep()
        {
            if (sweepFrame == 0) return;
            flipped = sweepFrame/2 % 2 == 1;
            sweepFrame = Math.Max((byte)(sweepFrame - 1), (byte)0);
        }

        private void Tip()
        {

        }

        private Animation idle = new Animation(new (byte, Frame)[]{
            (0, new Frame(new List<Triangle>()
            {
                // Bag
                new Triangle(7936,8194,7955,color:191),
                new Triangle(2818,3072,2835,color:191),
                new Triangle(3115,2835,7955,color:191),
                new Triangle(2835,3072,7955,color:191),
                new Triangle(3072,7936,7955,color:191),
                new Triangle(7955,8235,3115,color:191),
                new Triangle(8235,7725,3629,color:191),
                new Triangle(3629,3115,8235,color:191),

                // Crease
                new Triangle(5648,7952,8210,color:128),
                new Triangle(7955,5648,8210,color:128),

                // Eyes
                new Triangle(7708,7714,7202,color:26),
                new Triangle(6428,6434,5922,color:26),
                new Triangle(7196,7708,7202,color:26),
                new Triangle(5916,6428,5922,color:26),
            }))}, false);

        private Animation jump = new Animation(new (byte, Frame)[]{
            (0, new Frame(new List<Triangle>()
            {
                // Bag
                new Triangle(7939,8197,7444,color:191),
                new Triangle(3075,3585,2581,color:191),
                new Triangle(2859,2581,7444,color:191),
                new Triangle(2581,3585,7444,color:191),
                new Triangle(3585,7939,7444,color:191),
                new Triangle(7444,7723,2859,color:191),
                new Triangle(7723,7213,3373,color:191),
                new Triangle(3373,2859,7723,color:191),

                // Crease
                new Triangle(5138,7442,7699,color:128),
                new Triangle(7700,7445,5138,color:128),
                new Triangle(7443,7700,5138,color:128),
                new Triangle(7443,5138,7699,color:128),

                // Eyes
                new Triangle(7196,7202,6690,color:26),
                new Triangle(5916,5922,5410,color:26),
                new Triangle(6684,7196,6690,color:26),
                new Triangle(5404,5916,5410,color:26),

            }))}, false);

        private Animation dash = new Animation(new (byte, Frame)[]{
            (0, new Frame(new List<Triangle>()
            {
                // Bag
                new Triangle(2562,3073,8192,color:191),
                new Triangle(3093,2580,8212,color:191),
                new Triangle(13589,8212,8192,color:191),
                new Triangle(8212,2580,8192,color:191),
                new Triangle(2580,2562,8192,color:191),
                new Triangle(8192,13569,13589,color:191),
                new Triangle(13569,14083,14099,color:191),
                new Triangle(14099,13589,13569,color:191),

                // Crease
                new Triangle(8203,8468,8213,color:128),
                new Triangle(7956,8203,8213,color:128),

                // Eyes
                new Triangle(10753,11009,11013,color:26),
                new Triangle(10758,11014,11018,color:26),
                new Triangle(10757,10753,11013,color:26),
                new Triangle(10762,10758,11018,color:26),

            }))}, false);
        private Animation sweep = new Animation(new (byte, Frame)[]{
            (0, new Frame(new List<Triangle>()
            {
                // Bag
                new Triangle(11528,11530,9495,color:191),
                new Triangle(7169,8192,4881,color:191),
                new Triangle(3367,4881,9495,color:191),
                new Triangle(4881,8192,9495,color:191),
                new Triangle(8192,11528,9495,color:191),
                new Triangle(9495,7725,3367,color:191),
                new Triangle(7725,7214,3625,color:191),
                new Triangle(3625,3367,7725,color:191),

                // Crease
                new Triangle(7188,9749,10007,color:128),
                new Triangle(9753,9241,7188,color:128),
                new Triangle(9495,9753,7188,color:128),
                new Triangle(9495,7188,10007,color:128),

                // Eyes
                new Triangle(7965,7971,7459,color:26),
                new Triangle(6685,6691,6179,color:26),
                new Triangle(7453,7965,7459,color:26),
                new Triangle(6173,6685,6179,color:26),
            }))}, false);
    }
}