using UnityEngine;

namespace Fusion.XR.Host.Locomotion
{
    public interface ILocomotionValidator
    {
        public bool CanMoveHeadset(Vector3 headserNewPosition);
    }

    public interface ILocomotionObserver
    {
        public void OnDidMove();
        public void OnDidMoveFadeFinished();
    }
    public interface ILocomotionValidationHandler : ILocomotionValidator, ILocomotionObserver
    {
    }
}