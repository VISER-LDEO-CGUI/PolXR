using Fusion.XR.Host.Rig;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fusion.XR.Host
{
    /**
     * Helpers methods to facilitate the settings of InputActionProperty for XR controllers, to give them default action IF none were set in the inspector
     */
    public static class InputSystemExtensions
    {
        /**
         * Add default binding if none are already set (in reference or manually set in action), and add XR prefix for left and right bindings
         **/
        public static void EnableWithDefaultXRBindings(this InputActionProperty property, List<string> bindings = null, List<string> leftBindings = null, List<string> rightBindings = null)
        {
            if (property.reference ==null && property.action.bindings.Count == 0)
            {
                const string xrPrefix = "<XRController>";
                if (bindings == null) bindings = new List<string>();
                if (leftBindings != null) foreach (var binding in leftBindings) bindings.Add(xrPrefix + "{LeftHand}" + "/" + binding.TrimStart('/'));
                if (rightBindings != null) foreach (var binding in rightBindings) bindings.Add(xrPrefix + "{RightHand}" + "/" + binding.TrimStart('/'));

                foreach (var binding in bindings)
                {
                    property.action.AddBinding(binding);
                }
            }         

            if (property.action != null) property.action.Enable();
        }

        public static void EnableWithDefaultXRBindings(this InputActionProperty property, RigPart side, List<string> bindings)
        {
            if (side == RigPart.LeftController) property.EnableWithDefaultXRBindings(leftBindings: bindings);
            if (side == RigPart.RightController) property.EnableWithDefaultXRBindings(rightBindings: bindings);
        }
    }
}

