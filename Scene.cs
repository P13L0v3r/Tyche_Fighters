using static TycheFighters.Program;

namespace TycheFighters
{
    public partial class Game
    {
        public byte currentScene = 0;
        public bool gamePaused = false;

        // Menu Scene
        private void DrawMenuScene()
        {

        }

        private void UpdateMenuScene()
        {

        }

        // Fight Scene
        public Fighter[] fighters = new Fighter[2] { new Boxer(0, 64), new Dummy(1, 200) };

        public List<IInstance> extras = new List<IInstance>();
        public Bar bar1 = new Bar(PackWorldCoords(16, 235), 255, width: 64);
        public Bar bar2 = new Bar(PackWorldCoords(239, 239), 255, width: 64, reversed: true);
        private void DrawFightScene()
        {
            AddTrianglesToDraw(new Line(PackWorldCoords(0, (byte)(STAGE_FLOOR_HEIGHT - 2)), PackWorldCoords(255, (byte)(STAGE_FLOOR_HEIGHT - 2)), 1).Draw());

            AddTrianglesToDraw(fighters[0].Draw());
            AddTrianglesToDraw(fighters[1].Draw());

            foreach (IInstance instance in extras)
            {
                AddTrianglesToDraw(instance.Draw());
            }

            AddTrianglesToDraw(bar1.Draw());
            AddTrianglesToDraw(bar2.Draw());
        }

        private void UpdateFightScene()
        {
            foreach (Fighter fighter in fighters)
            {
                fighter.ReadKeyboard(KeyboardState);
                fighter.Update();
            }

            foreach (Collider A in fighters[0].colliders)
            {
                foreach (Collider B in fighters[1].colliders)
                {
                    if (Collider.Collide(A, B))
                    {
                        fighters[0].Collision(A, B);
                        fighters[1].Collision(B, A);
                    }
                }
            }

            extras = extras.FindAll(instance => !instance.IsDead());

            bar1.value = fighters[0].damage;
            bar2.value = fighters[1].damage;
        }
    }
}