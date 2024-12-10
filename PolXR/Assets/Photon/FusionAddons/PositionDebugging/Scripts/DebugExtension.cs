using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Addons.PositionDebugging
{
    public static class DebugExtension
    {
        public static GameObject DebugPrimitive(Vector3 position, Material material, PrimitiveType primitiveType = PrimitiveType.Cube, string name = "")
        {
            Mesh mesh = null;
            switch (primitiveType)
            {
                case PrimitiveType.Sphere:
                    mesh = Resources.GetBuiltinResource<Mesh>("New-Sphere.fbx");
                    break;
                case PrimitiveType.Cylinder:
                    mesh = Resources.GetBuiltinResource<Mesh>("New-Cylinder.fbx");
                    break;
                case PrimitiveType.Cube:
                    mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
                    break;
                default:
                    Debug.LogError("Unsupported type");
                    return null;
            }
            var c = new GameObject();
            c.AddComponent<MeshFilter>().sharedMesh = mesh;
            c.AddComponent<MeshRenderer>().sharedMaterial = material;


            c.transform.localScale = 0.01f * Vector3.one;
            c.transform.position = position;
            c.name = name == "" ? primitiveType.ToString() + "-" + Time.time : name;
            GameObject.Destroy(c.GetComponent<Collider>());
            return c;
        }
    }
}