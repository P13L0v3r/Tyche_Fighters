using static TycheFighters.Program;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using TycheFighters;

namespace TycheFighters
{
    public abstract class Fighter
    {
        public abstract byte damage { get; protected set; }
        public abstract byte weight { get; init; }
        public abstract byte width { get; init; }
        public abstract byte height { get; init; }
        protected abstract Keys[] P1map { get; init; }
        protected abstract Keys[] P2map { get; init; }
        public byte id { get; init; }
        public ushort position { get; protected set; }
        public bool flipped = false;
        public Collider hitBox { get; protected set; }
        public Frame currentFrame { get; protected set; }
        public bool knockedOut = false;
        public List<Collider> colliders;
        protected Random localRandom;

        public Fighter(byte id, byte startX)
        {
            this.id = id;
            this.position = PackWorldCoords(startX, STAGE_FLOOR_HEIGHT);
            this.hitBox = new Collider(0, Array.Empty<Triangle>()){owner = this};
            this.localRandom = new Random();
            this.colliders = new List<Collider>(){ hitBox };
        }

        public abstract void ReadKeyboard(KeyboardState keyState);

        public virtual void Update()
        {
            Launch();
        }

        public abstract void Collision(Collider internalCollider, Collider externalCollider);

        public virtual void TakeDamage(byte attack)
        {
            byte chance = (byte)Math.Clamp(localRandom.Next(256) * weight, 0, 255);

            GAME_INSTANCE.extras.Add(new Burst((ushort)(position - PackWorldCoords(6, 6) + PackWorldCoords((byte)localRandom.Next(width), (byte)localRandom.Next(height))), 5));
            //GAME_INSTANCE.shakeFrame = 4;
            GAME_INSTANCE.shakeStrength += attack;
            //GAME_INSTANCE.extras.Add(new Burst(position, 5));

            if (chance < damage)
            {
                Knockout();
            }
            else
            {
                damage = (byte)Math.Clamp(damage + attack, 0, 255);
            }
        }

        private byte launchFrame = 0;
        private ushort launchDirection = 0;

        public void InitLaunch(sbyte x, sbyte y)
        {
            launchDirection = (ushort)((x << 8) | y);
            launchFrame = 16;
        }
        public virtual void Launch()
        {
            if (launchFrame == 0)
            {
                return;
            }

            byte newX = (byte)(position >> 8);
            byte newY = (byte)position;
            newX = (byte)Math.Clamp(newX + (sbyte)(launchDirection >> 8) * launchFrame, 0, 255 - width);
            newY = (byte)Math.Clamp(newY + (sbyte)launchDirection * launchFrame, STAGE_FLOOR_HEIGHT, 255 - height);
            if (newX == 255-width || newX == 0)
            {
                launchDirection = (ushort)((-1 * (launchDirection >> 8) << 8) | (sbyte)launchDirection);
            }

            if (newY == 255-height || newY == STAGE_FLOOR_HEIGHT)
            {
                launchDirection = (ushort)((launchDirection >> 8) << 8 | (sbyte)(-1 * launchDirection));
            }

            position = PackWorldCoords( newX, newY );

            launchFrame = (byte)Math.Max(launchFrame - 1, 0);

            if (launchFrame == 0) fallFrame = 1;
        }

        protected byte fallFrame = 0;

        public virtual void Knockout()
        {
            this.knockedOut = true;
            this.fallFrame = 1;
        }

        public List<Triangle> Draw()
        {
            //List<Triangle> result = new List<Triangle>(currentFrame.triangles);
            Triangle[] result = new Triangle[currentFrame.triangles.Count];

            for(int i = 0; i < result.Length; i++)
            {
                result[i] = flipped ? currentFrame.triangles[i].Flip(width) + position : currentFrame.triangles[i] + position;
            }

            return result.ToList();
        }

        protected ushort scrollingSequence { get; set; } = 0;
        protected Combo[] allComboes { get; init; } = { };
        protected Action[] comboResults { get; init; } = { };

        protected void AddKey(byte keyIndex)
        {
            scrollingSequence = (ushort)(scrollingSequence << 3 | keyIndex);
        }

        protected void CheckCombo()
        {
            for (byte i = 0; i < allComboes.Length; i++)
            {
                if (allComboes[i] == scrollingSequence)
                {
                    comboResults[i]();
                    scrollingSequence = 0;
                    return;
                }
            }
        }
    }

    [Serializable]
    public struct Pose
    {
        public ushort[] vertices; // Vertex pairs signify lines

        public Pose(ushort[] vertices)
        {
            this.vertices = vertices;
        }
    }

    [Serializable]
    public struct Frame
    {
        public readonly List<Triangle> triangles;

        public Frame(List<Triangle> triangles)
        {
            this.triangles = triangles;
        }

        public Frame(Pose pose)
        {
            triangles = new List<Triangle>();
            for(int i=0; i < pose.vertices.Length / 2; i++)
            {
                triangles = Enumerable.Concat(triangles, new Line(pose.vertices[i*2], pose.vertices[i*2+1], 1).Draw()).ToList();
            }
        }
    }

    [Serializable]
    public class Animation
    {
        private Frame[] keyframes;
        private ushort[] frameNumbers;
        private ushort currentFrame = 0;
        private byte currentIndex = 0;
        public bool loop = true;

        public Animation((byte, Frame)[] frames, bool loop = true)
        {
            this.keyframes = new Frame[frames.Length];
            this.frameNumbers = new ushort[frames.Length];

            for (int i = 0; i < frames.Length; i++)
            {
                this.frameNumbers[i] = frames[i].Item1;
                this.keyframes[i] = frames[i].Item2;
            }

            this.loop = loop;
        }

        public void Start()
        {
            currentIndex = 0;
            currentFrame = 0;
        }

        public void ActivateOnFrame(byte frame, Action frameTrigger)
        {
            if (currentFrame == frame)
            {
                frameTrigger();
            }
        }

        public bool Finished()
        {
            return currentIndex == keyframes.Length - 1 && !loop;
        }

        public void NextFrame()
        {
            currentFrame++;
        }

        public Frame CurrentFrame()
        {
            if (currentIndex + 1 < frameNumbers.Length)
            {
                if (currentFrame < frameNumbers[currentIndex + 1])
                {
                    return keyframes[currentIndex];
                }
                else
                {
                    currentIndex++;
                    return CurrentFrame();
                }
            }
            else
            {
                if (loop)
                {
                    currentIndex = 0;
                    currentFrame = 0;
                }

                return keyframes[currentIndex];
            }
        }
    }
}

public class Collider
{
    public byte id { get; private set; }
    public byte attack;
    public ushort origin;
    private Triangle[] triangles;
    public Fighter owner;
    public Collider(byte id, Triangle[] triangles, byte attack = 0, ushort origin = 0)
    {
        this.id = id;
        this.attack = attack;
        this.triangles = triangles;
    }

    public static bool Collide(Collider first, Collider second)
    {
        foreach (Triangle A in first.triangles)
        {
            foreach (Triangle B in second.triangles)
            {
                if (Triangle.Intersect(A + first.origin, B + second.origin))
                {
                    // If any triangle in the first collider intersects any triangle in the second colldier, then both colliders collide
                    return true;
                }
            }
            
        }
        return false;
    }

    public void Flag()
    {
        id |= 0b10000000;
    }

    public bool IsFlagged()
    {
        return (id & 0b10000000) == 0b10000000;
    }

    public void ClearFlag()
    {
        id &= 0b01111111;
    }
}