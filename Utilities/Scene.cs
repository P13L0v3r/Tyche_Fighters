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
        public Fighter[] fighters = new Fighter[2] { new TheBag(0, 64), new Boxer(1, 200) };

        public List<IInstance> extras = new List<IInstance>();
        public Bar bar1 = new Bar(PackWorldCoords(16, 235), 255, width: 64);
        public Bar bar2 = new Bar(PackWorldCoords(239, 239), 255, width: 64, reversed: true);

        public Text label1 = new Text("0%", PackWorldCoords(16, 221));
        public Text label2 = new Text("0%", PackWorldCoords(175, 221));

        public Text name1 = new Text("PLAYER 1", PackWorldCoords(16, 208));
        public Text name2 = new Text("PLAYER 2", PackWorldCoords(175, 208));

        public Text knockout = new Text("KNOCKOUT", PackWorldCoords(45, 128), 3);
        public Text winner = new Text("", PackWorldCoords(38, 100), 2);

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

            AddTrianglesToDraw(label1.Draw());
            AddTrianglesToDraw(label2.Draw());

            AddTrianglesToDraw(name1.Draw());
            AddTrianglesToDraw(name2.Draw());

            if (fighters[0].knockedOut || fighters[1].knockedOut)
            {
                AddTrianglesToDraw(knockout.Draw());
                AddTrianglesToDraw(winner.Draw());
            }

            shakeFrame = shakeFrame < 16 ? (byte)(shakeFrame + 1) : (byte)0;

            shakeStrength = shakeStrength < 0.01f ? 0.0f : shakeStrength / 2f;
        }

        private void UpdateFightScene()
        {
            if (!fighters[0].knockedOut) fighters[0].ReadKeyboard(KeyboardState);

            fighters[0].Update();

            if (!fighters[1].knockedOut) fighters[1].ReadKeyboard(KeyboardState);

            fighters[1].Update();

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

            label1.text = (bar1.value / 2.55f).ToString("0.0#") + "%";
            label2.text = (bar2.value / 2.55f).ToString("0.0#") + "%";

            if (fighters[0].knockedOut)
            {
                //gamePaused = true;
                //Console.WriteLine("P1 Knocked out, P2 Wins!");
                winner.text = "PLAYER 2 WINS";
            }
            else if (fighters[1].knockedOut)
            {
                //gamePaused = true;
                //Console.WriteLine("P2 Knocked out, P1 Wins!");
                winner.text = "PLAYER 1 WINS";
            }
        }
    }
}