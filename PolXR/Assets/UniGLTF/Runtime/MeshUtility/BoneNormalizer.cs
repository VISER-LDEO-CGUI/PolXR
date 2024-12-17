﻿using System;
using System.Collections.Generic;
using System.Linq;
using UniGLTF.Utils;
using UnityEngine;


namespace UniGLTF.MeshUtility
{
    public static class BoneNormalizer
    {
        private static MeshAttachInfo CreateMeshInfo(Transform src, bool bakeCurrentBlendShape)
        {
            // SkinnedMeshRenderer
            if (src.TryGetComponent<SkinnedMeshRenderer>(out var smr))
            {
                var mesh = MeshFreezer.NormalizeSkinnedMesh(smr, bakeCurrentBlendShape);
                if (mesh != null)
                {
                    return new MeshAttachInfo
                    {
                        Mesh = mesh,
                        Materials = smr.sharedMaterials,
                        Bones = smr.bones,
                        RootBone = smr.rootBone,
                    };
                }
            }

            // MeshRenderer
            if (src.TryGetComponent<MeshRenderer>(out var mr))
            {
                var dstMesh = MeshFreezer.NormalizeNoneSkinnedMesh(mr, true);
                if (dstMesh != null)
                {
                    return new MeshAttachInfo
                    {
                        Mesh = dstMesh,
                        Materials = mr.sharedMaterials,
                    };
                }
            }

            return default;
        }


        /// <summary>
        /// 各レンダラー(SkinnedMeshRenderer と MeshRenderer)にアタッチされた sharedMesh に対して
        /// 回転とスケールを除去し、BlendShape の現状を焼き付けた版を作成する(まだ、アタッチしない)
        /// </summary>
        public static Dictionary<Transform, MeshAttachInfo> NormalizeHierarchyFreezeMesh(
            GameObject go, bool bakeCurrentBlendShape)
        {
            var result = new Dictionary<Transform, MeshAttachInfo>();
            foreach (var src in go.transform.Traverse())
            {
                var info = CreateMeshInfo(src, bakeCurrentBlendShape);
                if (info != null)
                {
                    result.Add(src, info);
                }
            }
            return result;
        }

        public static void Replace(GameObject go, Dictionary<Transform, MeshAttachInfo> meshMap,
            bool KeepRotation)
        {
            var boneMap = go.transform.Traverse().ToDictionary(x => x, x => new EuclideanTransform(x.rotation, x.position));

            // first, update hierarchy
            foreach (var src in go.transform.Traverse())
            {
                var tr = boneMap[src];
                src.localScale = Vector3.one;

                if (KeepRotation)
                {
                    src.rotation = tr.Rotation;
                }
                else
                {
                    src.rotation = Quaternion.identity;
                }

                src.position = tr.Translation;
            }

            // second, replace mesh
            foreach (var (src, tr) in boneMap)
            {
                if (meshMap.TryGetValue(src, out var info))
                {
                    info.ReplaceMesh(src.gameObject);
                }
            }
        }
    }
}
