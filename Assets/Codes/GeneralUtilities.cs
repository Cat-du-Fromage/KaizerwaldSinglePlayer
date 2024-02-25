using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kaizerwald.FormationModule;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Kaizerwald
{
    public static class GeneralUtilities
    {
        /*
        // =============================================================================================================
        // Dictionary
        // =============================================================================================================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearSafe<TKey,TValue>(this Dictionary<TKey,TValue> dictionary, int newCapacity = 0)
        {
            dictionary ??= new Dictionary<TKey,TValue>(newCapacity);
            dictionary.Clear();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<int> Iota(int length, Allocator allocator)
        {
            NativeArray<int> nativeArray = new (length, allocator, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < length; i++) nativeArray[i] = i;
            return nativeArray;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(this NativeArray<T> nativeArray, int left, int right)
        where T : unmanaged
        {
            (nativeArray[left], nativeArray[right]) = (nativeArray[right], nativeArray[left]);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(this NativeList<T> nativeList, int left, int right)
        where T : unmanaged
        {
            (nativeList[left], nativeList[right]) = (nativeList[right], nativeList[left]);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T1,T2>(this NativeParallelHashMap<T1,T2> nativeArray, T1 left, T1 right)
            where T1 : unmanaged, IEquatable<T1>
            where T2 : unmanaged
        {
            (nativeArray[left], nativeArray[right]) = (nativeArray[right], nativeArray[left]);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Reverse<T>(this NativeArray<T> nativeArray)
        where T : unmanaged
        {
            int midLength = nativeArray.Length / 2;
            for (int i = 0; i < midLength; i++)
            {
                int endIndex = nativeArray.Length - 1 - i;
                (nativeArray[i], nativeArray[endIndex]) = (nativeArray[endIndex], nativeArray[i]);
            }
        }
        
        public static bool IsObjectBBehindObjectA(float2 currentPosition, float2 targetPosition, float2 currentFormationDir) //A - B
        {
            float2 directionCurrentToTarget = math.normalizesafe(targetPosition - currentPosition);
            float dotProduct = math.dot(directionCurrentToTarget, currentFormationDir);
            return dotProduct <= -0.5f; // Check if the dot product is less than the threshold
        }
        
        public static bool IsObjectALookingOppositeDirectionOfObjectB(float2 targetFormationDir, float2 currentFormationDir)
        {
            // Calculate the dot product
            float dotProduct = math.dot(targetFormationDir, currentFormationDir);
            // Define a threshold for considering directions as opposite
            float oppositeThreshold = -0.95f; // Adjust as needed based on your tolerance
            // Check if the dot product is less than the threshold
            return dotProduct <= oppositeThreshold;
        }
        
        public static float3 GetLeaderOppositeDirection(in float3 leaderPosition, in FormationData formation)
        {
            return leaderPosition + formation.DistanceUnitToUnitY * formation.Depth * formation.Direction3DBack;
        }
        */
    }
    
}
