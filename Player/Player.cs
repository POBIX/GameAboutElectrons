using Godot;

public class Player : KinematicBody2D
{
    public Vector2 velocity;

    private const int Acceleration = 50;
    private const int AirAcceleration = 5;
    private const float Friction = 0.2f;
    private const float AirFriction = 0.03f;
    private const int MaxSpeed = 350;
    private const int JumpForce = 500;
    private const int JumpSmooth = 250;
    private const int ZapDistance = 325;
    private const int TerminalVelocity = 1000;
    private const int ZapSpeed = 1000;
    private const int WallJumpForce = 450;
    private const int WallJumpKick = 300;
    private const int MinimumHookLength = 80;

    private Sprite sprite;
    private AnimationPlayer anim;
    private AnimationPlayer swapAnim;
    private Area2D protonField;
    private Line2D protonLine;
    private RayCast2D protonRay;
    private Level level;

    private Proton closest;
    private Proton prevProton;
    private RayCast2D wallL;
    private RayCast2D wallR;
    private RayCast2D wallCurr;

    private AnimatedTexture lightningTexture;

    private float hookDistance;

    public enum Particle
    {
        Electron,
        Proton
    }

    public Particle Mode { get; private set; }

    public enum State
    {
        Idle,
        Walk,
        Jump,
        Fall,
        Zap,
        Bounce,
        Hook,
        WallJump,
        WallSlide
    }

    private State state = State.Idle;

    public State GetState() => state;

    public void SetState(State s)
    {
        state = s;
        Start();
    }

    public override void _Ready()
    {
        base._Ready();

        sprite = GetNode<Sprite>("Sprite");
        anim = GetNode<AnimationPlayer>("AnimationPlayer");
        swapAnim = GetNode<AnimationPlayer>("SwapAnimationPlayer");
        protonField = GetNode<Area2D>("ProtonField");
        protonField.Scale = new Vector2(ZapDistance, ZapDistance);
        lightningTexture = ResourceLoader.Load<AnimatedTexture>("res://Player/Lightning/Lightning.tres");
        wallL = GetNode<RayCast2D>("WJL");
        wallR = GetNode<RayCast2D>("WJR");
        level = GetParent<Level>();
        protonLine = GetNode<Line2D>("../ProtonLine");
        protonRay = GetNode<RayCast2D>("ProtonRay");

        Mode = Particle.Electron;
        swapAnim.Play(Mode.ToString());
        swapAnim.Advance(0.2f);
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        wallL.GlobalRotation = wallR.GlobalRotation = 0;

        if (GetState() != State.Zap && GetState() != State.Hook)
        {
            UpdateZap();
            HookAndZap();
        }
        UpdateLine();

        switch (GetState())
        {
            case State.Idle:
                Move();
                if (velocity.x != 0)
                    SetState(State.Walk);
                break;
            case State.Jump:
                Move(airAcceleration: 20, airFriction: 0.05f);
                if (!Input.IsActionPressed("Jump"))
                {
                    if (velocity.y < -JumpSmooth)
                        velocity.y = -JumpSmooth;
                }

                if (velocity.y >= 0)
                    SetState(State.Fall);
                break;
            case State.Walk:
                Move();
                if (velocity == new Vector2())
                    SetState(State.Idle);
                break;
            case State.Fall:
                Move(airAcceleration: 20, airFriction: 0.05f);
                if (IsOnFloor())
                    SetState(State.Idle);
                break;
            case State.Zap:
                prevProton = closest;

                if (closest == null)
                {
                    SetState(State.Idle);
                    return;
                }
                const int distanceThreshold = 64;

                velocity = GlobalPosition.DirectionTo(closest.GlobalPosition) * ZapSpeed;

                RotateTween(velocity.Angle() + Mathf.Pi / 2);

                // if inside of proton
                if (GlobalPosition.DistanceTo(closest.GlobalPosition) < distanceThreshold)
                {
                    SetState(State.Bounce);

                    protonLine.Width = 1;
                    protonLine.Texture = null;
                }

                break;
            case State.Bounce:
                Move(AirAcceleration, AirFriction, ZapSpeed);
                if (IsOnFloor() || IsOnWall() || IsOnCeiling())
                    SetState(State.Idle);
                break;
            case State.Hook:
                Move(maxSpeed: 2000, friction: 0.02f, air: false, jump: IsOnFloor());

                Vector2 target = closest.GlobalPosition;
                Vector2 pos = GlobalPosition;
                Vector2 prime = pos + velocity * delta;

                if (prime.DistanceTo(target) >= hookDistance)
                {
                    Vector2 v = velocity - velocity.Dot(pos - target) / pos.DistanceSquaredTo(target) * (pos - target);

                    Vector2 prime2 = pos + v * delta;
                    Vector2 newPos = target + (prime2 - target) * (pos.DistanceTo(target) / prime2.DistanceTo(target));
                    velocity = (newPos - pos) / delta;
                    hookDistance = newPos.DistanceTo(target);
                }

                void ResetLine()
                {
                    protonLine.Width = 1;
                    protonLine.Texture = null;
                }

                if (!Input.IsActionPressed("Grapple"))
                {
                    SetState(State.Bounce);
                    ResetLine();
                }
                break;

            case State.WallJump:
                Move();
                if (!Input.IsActionPressed("Jump"))
                {
                    if (velocity.y < -JumpSmooth)
                    {
                        velocity.y = -JumpSmooth;
                        velocity.x -= 50 * Mathf.Sign(velocity.x);
                    }
                    SetState(State.Fall);
                }

                break;

            case State.WallSlide:
                Move(gravity: velocity.y > 0 ? 10 : Physics.Gravity);

                if (wallCurr == null || IsOnFloor() || !wallCurr.IsColliding())
                {
                    SetState(State.Idle);
                    break;
                }
                if (Input.IsActionJustPressed("Jump"))
                    SetState(State.WallJump);

                sprite.FlipH = !wallCurr.Name.EndsWith('L');

                break;
        }

        velocity = MoveAndSlide(velocity, Vector2.Up);
        if (IsOnFloor())
        {
            sprite.RotationDegrees += velocity.x * delta;
            if (GetState() != State.Hook && GetState() != State.Zap)
                prevProton = null;
        }

        if (GlobalPosition.y > level.deathHeight) Die();

        if (Input.IsActionJustPressed("Quit") && !Pause.annoyingBug)
        {
            GetTree().Paused = true;
            Spawner.Node("res://UI/Pause.tscn", GetParent());
        }

        Pause.annoyingBug = false;
    }

    private int lastMoveSign = 1;
    private void UpdateZap()
    {
        const int signCost = 50;

        int sign = Mathf.Sign(velocity.x);
        if (sign == 0) sign = lastMoveSign;
        else lastMoveSign = sign;

        protonLine.ClearPoints();
        closest = null;

        // A proton is only reachable if it's in range of the circle's area + any number
        float closestDist = Mathf.Pi * ZapDistance * ZapDistance + 0.69420f;
        foreach (Proton body in protonField.GetOverlappingAreas())
        {
            float dist = GlobalPosition.DistanceTo(body.GlobalPosition);

            // prioritize protons in the movement direction
            Vector2 dir = GlobalPosition.DirectionTo(body.GlobalPosition);
            if (dir.x * sign > 0)
                dist -= signCost;

            if (Mathf.Abs(dist) < closestDist && body != prevProton)
            {
                protonRay.CastTo = ToLocal(body.GlobalPosition);
                if (protonRay.IsColliding()) continue;
                closestDist = dist;
                closest = body;
            }
        }
    }

    private void UpdateLine()
    {
        protonLine.ClearPoints();

        if (closest != null)
        {
            protonLine.AddPoint(GlobalPosition);
            protonLine.AddPoint(closest.GlobalPosition);
        }
    }

    private void HookAndZap()
    {
        // if (level.swap && Input.IsActionJustPressed("Swap"))
        // {
        //     Mode = Mode == Particle.Electron ? Particle.Proton : Particle.Electron;
        //     swapAnim.Play(Mode.ToString());
        // }

        if (closest == null) return;

        if (level.zap && Input.IsActionJustPressed("Zap")) SetState(State.Zap);
        else if (Input.IsActionPressed("Grapple")) SetState(State.Hook);
    }

    private void Move(float acceleration = Acceleration, float friction = Friction, int maxSpeed = MaxSpeed,
                      bool air = true, float gravity = Physics.Gravity, float airAcceleration = AirAcceleration,
                      float airFriction = AirFriction, bool jump = true)
    {
        bool grounded = IsOnFloor() && air;
        if (Input.IsActionPressed("MoveLeft"))
        {
            velocity.x -= grounded ? acceleration : airAcceleration;
            if (Rotation > -Mathf.Pi / 8)
                sprite.FlipH = false;
        }
        else if (Input.IsActionPressed("MoveRight"))
        {
            velocity.x += grounded ? acceleration : airAcceleration;
            if (Rotation < Mathf.Pi / 8)
                sprite.FlipH = true;
        }
        else if (friction != 0) velocity.x = (int)Mathf.Lerp(velocity.x, 0, grounded ? friction : airFriction);

        velocity.x = Mathf.Clamp(velocity.x, -maxSpeed, maxSpeed);
        velocity.y = Mathf.Min(velocity.y, TerminalVelocity);

        if (!IsOnFloor() && GetState() != State.WallSlide && GetState() != State.Hook)
        {
            if (wallL.IsColliding())
            {
                wallCurr = wallL;
                SetState(State.WallSlide);
                return;
            }
            if (wallR.IsColliding())
            {
                wallCurr = wallR;
                SetState(State.WallSlide);
                return;
            }
            wallCurr = null;
        }

        if (jump && IsOnFloor() && Input.IsActionJustPressed("Jump"))
            SetState(State.Jump);

        velocity.y += gravity;
    }

    private void Start()
    {
        switch (GetState())
        {
            case State.Jump:
                velocity.y -= JumpForce;
                break;
            case State.Zap:
                Sound.Play("zap.wav", this, 20);
                protonLine.Width = 128;
                protonLine.Texture = lightningTexture;
                if (Mode != Particle.Proton)
                {
                    swapAnim.Play("Proton");
                    Mode = Particle.Proton;
                }
                break;
            case State.Hook:
                Sound.Play("grapple.wav", this, 15);
                protonLine.Width = 32;
                protonLine.Texture = lightningTexture;
                if (Mode != Particle.Electron)
                {
                    swapAnim.Play("Electron");
                    Mode = Particle.Electron;
                }
                RotateTween(0);
                hookDistance = Mathf.Max(GlobalPosition.DistanceTo(closest.GlobalPosition), MinimumHookLength);
                prevProton = closest;
                break;
            case State.WallSlide:
                RotateTween(0);
                break;
            case State.WallJump:
                velocity.x = -WallJumpKick * Mathf.Sign(wallCurr.CastTo.x);
                velocity.y = -WallJumpForce;
                break;
        }

        string name = GetState().ToString();
        anim.Play(anim.HasAnimation(name) ? name : "Idle");
    }

    private async void RotateTween(float newRot)
    {
        var tween = new Tween();
        AddChild(tween);
        tween.InterpolateProperty(sprite, "rotation", Rotation, newRot, 0.05f, Tween.TransitionType.Quad);
        tween.Start();
        await ToSignal(tween, "tween_all_completed");
        tween.QueueFree();
    }

    public void Die() => Spawner.Fade(this, Fade.In, () => GetTree().ReloadCurrentScene());

    public void NextLevel()
    {
        string curr = GetParent().Filename;
        int num = int.Parse(curr[^6].ToString());
        string next = curr.Remove(curr.Length - 6, 1);
        next = next.Insert(next.Length - 5, (num + 1).ToString());
        GetTree().ChangeScene(next);
    }
}
