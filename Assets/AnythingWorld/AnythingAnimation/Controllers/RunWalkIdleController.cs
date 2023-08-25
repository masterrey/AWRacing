namespace AnythingWorld.Animation
{
    public class RunWalkIdleController : LegacyAnimationController
    {
        private AnimationState currentState = AnimationState.idle;

        public void BlendAnimationOnSpeed(float speed, float walkThreshold, float runThreshold)
        {


            if (speed > runThreshold)
            {
                Run();
            }
            else if (speed > walkThreshold)
            {
                Walk();
            }
            else
            {
                Idle();
            }
        }

        public void BlendAnimationOnSpeed(float speed, float walkThreshold)
        {
            if (speed > walkThreshold)
            {
                Walk();
            }
            else
            {
                Idle();
            }
        }
        public void Walk()
        {
            if (currentState != AnimationState.walk)
            {
                base.CrossFadeAnimation("walk");
                currentState = AnimationState.walk;
            }

        }
        public void Run()
        {
            if (currentState != AnimationState.run)
            {
                base.CrossFadeAnimation("run");
                currentState = AnimationState.run;
            }
        }
        public void Idle()
        {
            if (currentState != AnimationState.idle)
            {
                base.CrossFadeAnimation("idle");
                currentState = AnimationState.idle;
            }
        }
        private enum AnimationState
        {
            idle,
            walk,
            run
        }

    }
}
