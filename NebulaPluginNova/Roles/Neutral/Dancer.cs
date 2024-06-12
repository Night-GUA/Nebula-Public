﻿using Nebula.Behaviour;
using Nebula.Modules.GUIWidget;
using Virial;
using Virial.Assignable;
using Virial.Events.Game;

namespace Nebula.Roles.Neutral;

public class Dancer : DefinedRoleTemplate, DefinedRole
{
    static public RoleTeam MyTeam = new Team("teams.dancer", new(255, 255, 255), TeamRevealType.OnlyMe);

    private Dancer() : base("dancer", MyTeam.Color, RoleCategory.NeutralRole, MyTeam, []) { }

    static public Dancer MyRole = new Dancer();

    RuntimeRole RuntimeAssignableGenerator<RuntimeRole>.CreateInstance(GamePlayer player, int[] arguments) => new Instance(player);

    static public float DancePlayerRange => 3f;
    static public float DanceCorpseRange => 3f;
    static public float DanceDuration => 3f;
    public class DanceProgress
    {
        public Vector2 Position { get; private set; }
        public EditableBitMask<GamePlayer> Players = BitMasks.AsPlayer();
        public EditableBitMask<GamePlayer> Corpses = BitMasks.AsPlayer();
        public float Progress { get; private set; }
        private float failProgress = 0f;
        public bool IsFailed { get; private set; } = false;
        public bool IsCompleted { get; private set; } = false;
        private EffectCircle Effect;
        public void Update(bool isDancing)
        {
            if (IsCompleted || IsFailed) return;

            if (isDancing)
            {
                Progress += Time.deltaTime;
                if (Progress > DanceDuration)
                {
                    IsCompleted = true;
                    OnFinished();
                }
            }
            else
            {
                failProgress += Time.deltaTime;
                if (failProgress > 0.4f)
                {
                    IsFailed = true;
                    OnFinished();
                }
                
            }
        }

        private void OnFinished()
        {
            if (Effect) Effect.Disappear();
        }

        public void Destroy()
        {
            if (Effect) Effect.DestroyFast();
        }

        public DanceProgress(Vector2 pos)
        {
            Position = pos;
            Effect = EffectCircle.SpawnEffectCircle(null, pos, MyRole.UnityColor, DancePlayerRange, null, true);
        }
    }

    public class Instance : RuntimeAssignableTemplate, RuntimeRole
    {
        DefinedRole RuntimeRole.Role => MyRole;

        public Instance(GamePlayer player) : base(player)
        {
        }

        void RuntimeAssignable.OnActivated()
        {
        }

        void RuntimeAssignable.OnInactivated()
        {
            currentDance?.Destroy();
        }
        

        DanceProgress? currentDance = null;

        Vector2? lastPos = null;
        Vector2 displacement = new();
        float distance = 0f;
        float danceGuage = 0f;

        [Local]
        void OnUpdate(GameUpdateEvent ev)
        {
            if (AmOwner)
            {
                Vector2 currentPos = MyPlayer.VanillaPlayer.transform.position;
                if (lastPos != null)
                {
                    distance *= 0.89f;
                    distance += currentPos.Distance(lastPos.Value);

                    displacement *= 0.89f;
                    displacement += currentPos - lastPos.Value;
                }
                lastPos = currentPos;

                if (distance > 0.3f && displacement.magnitude < 0.18f)
                    danceGuage = Math.Min(danceGuage + Time.deltaTime * 4.2f, 1f);
                else
                    danceGuage = Math.Max(danceGuage - Time.deltaTime * 2.7f, 0f);


                if (currentDance != null)
                {
                    if (currentDance.IsCompleted)
                    {
                        currentDance = null;
                    }
                    else if (currentDance.IsFailed)
                        currentDance = null;
                }

                if (IsDancing) currentDance ??= new DanceProgress(MyPlayer.Position);
                currentDance?.Update(IsDancing);
            }
        }

        bool IsDancing => danceGuage > 0.7f;
    }
}