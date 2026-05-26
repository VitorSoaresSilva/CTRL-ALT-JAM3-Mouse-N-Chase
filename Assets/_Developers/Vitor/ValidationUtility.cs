using UnityEngine;

namespace _Developers.Vitor
{
    /// <summary>
    /// Utilitário para validar valores e evitar erros Invalid AABB
    /// </summary>
    public static class ValidationUtility
    {
        /// <summary>
        /// Valida se um Vector3 é válido (não NaN, não Infinity, não absurdamente grande)
        /// </summary>
        public static bool IsValidVector3(Vector3 vector, float maxMagnitude = 10000f)
        {
            // Verificar NaN
            if (float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z))
            {
                Debug.LogWarning($"[ValidationUtility] Vector3 inválido (NaN): {vector}");
                return false;
            }

            // Verificar Infinity
            if (float.IsInfinity(vector.x) || float.IsInfinity(vector.y) || float.IsInfinity(vector.z))
            {
                Debug.LogWarning($"[ValidationUtility] Vector3 inválido (Infinity): {vector}");
                return false;
            }

            // Verificar magnitude absurda
            if (vector.magnitude > maxMagnitude)
            {
                Debug.LogWarning($"[ValidationUtility] Vector3 com magnitude absurda: {vector} (mag: {vector.magnitude})");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida se uma Quaternion é válida (não NaN, não inválida)
        /// </summary>
        public static bool IsValidQuaternion(Quaternion quaternion)
        {
            // Verificar NaN
            if (float.IsNaN(quaternion.x) || float.IsNaN(quaternion.y) || float.IsNaN(quaternion.z) || float.IsNaN(quaternion.w))
            {
                Debug.LogWarning($"[ValidationUtility] Quaternion inválido (NaN): {quaternion}");
                return false;
            }

            // Verificar Infinity
            if (float.IsInfinity(quaternion.x) || float.IsInfinity(quaternion.y) || float.IsInfinity(quaternion.z) || float.IsInfinity(quaternion.w))
            {
                Debug.LogWarning($"[ValidationUtility] Quaternion inválido (Infinity): {quaternion}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida se uma escala é válida (não zero, não negativa absurda, não Infinity)
        /// </summary>
        public static bool IsValidScale(Vector3 scale, float minScale = 0.01f, float maxScale = 100f)
        {
            // Verificar NaN
            if (float.IsNaN(scale.x) || float.IsNaN(scale.y) || float.IsNaN(scale.z))
            {
                Debug.LogWarning($"[ValidationUtility] Escala inválida (NaN): {scale}");
                return false;
            }

            // Verificar Infinity
            if (float.IsInfinity(scale.x) || float.IsInfinity(scale.y) || float.IsInfinity(scale.z))
            {
                Debug.LogWarning($"[ValidationUtility] Escala inválida (Infinity): {scale}");
                return false;
            }

            // Verificar se está nos limites
            if (scale.x < minScale || scale.y < minScale || scale.z < minScale)
            {
                Debug.LogWarning($"[ValidationUtility] Escala muito pequena: {scale}");
                return false;
            }

            if (scale.x > maxScale || scale.y > maxScale || scale.z > maxScale)
            {
                Debug.LogWarning($"[ValidationUtility] Escala muito grande: {scale}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida se uma velocidade é válida
        /// </summary>
        public static bool IsValidVelocity(Vector3 velocity, float maxVelocity = 500f)
        {
            if (!IsValidVector3(velocity, maxVelocity))
            {
                Debug.LogWarning($"[ValidationUtility] Velocidade inválida: {velocity}");
                return false;
            }

            if (velocity.magnitude > maxVelocity)
            {
                Debug.LogWarning($"[ValidationUtility] Velocidade absurda: {velocity.magnitude} (máx: {maxVelocity})");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Clamp um Vector3 para valores válidos
        /// </summary>
        public static Vector3 ClampVector3(Vector3 vector, float maxMagnitude = 10000f)
        {
            if (float.IsNaN(vector.x) || float.IsInfinity(vector.x)) vector.x = 0;
            if (float.IsNaN(vector.y) || float.IsInfinity(vector.y)) vector.y = 0;
            if (float.IsNaN(vector.z) || float.IsInfinity(vector.z)) vector.z = 0;

            if (vector.magnitude > maxMagnitude)
            {
                return vector.normalized * maxMagnitude;
            }

            return vector;
        }

        /// <summary>
        /// Clamp uma velocidade para valores seguros
        /// </summary>
        public static Vector3 ClampVelocity(Vector3 velocity, float maxVelocity = 500f)
        {
            velocity = ClampVector3(velocity, maxVelocity);

            if (velocity.magnitude > maxVelocity)
            {
                return velocity.normalized * maxVelocity;
            }

            return velocity;
        }
    }
}
