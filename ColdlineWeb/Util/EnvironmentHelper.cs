using System;

namespace ColdlineWeb.Util
{
    /// <summary>
    /// Helper para acessar variáveis de ambiente do sistema.
    /// Centraliza a lógica e garante consistência no uso.
    /// </summary>
    public static class EnvironmentHelper
    {
        private const string FixedDepartmentId = "68bb3ba7852c9b2a93bd75b7";
        private const string FixedUserTypeId   = "68dc2a1e6407ff079542f8e1";

        /// <summary>
        /// Obtém o ID fixo do departamento a partir da variável de ambiente COLDLINE_FILTER_DEPARTMENT_ID.
        /// Se não existir, usa o valor padrão definido em código.
        /// </summary>
        public static string GetDepartmentId()
        {
            var departmentId = Environment.GetEnvironmentVariable("COLDLINE_FILTER_DEPARTMENT_ID");
            return string.IsNullOrWhiteSpace(departmentId) ? FixedDepartmentId : departmentId;
        }

        /// <summary>
        /// Obtém o ID fixo do tipo de usuário a partir da variável de ambiente COLDLINE_FILTER_USER_TYPE_ID.
        /// Se não existir, usa o valor padrão definido em código.
        /// </summary>
        public static string GetUserTypeId()
        {
            var userTypeId = Environment.GetEnvironmentVariable("COLDLINE_FILTER_USER_TYPE_ID");
            return string.IsNullOrWhiteSpace(userTypeId) ? FixedUserTypeId : userTypeId;
        }

        /// <summary>
        /// Permite ler qualquer variável de ambiente de forma segura.
        /// </summary>
        public static string? GetEnvironmentVariable(string variableName, bool throwIfNotFound = false)
        {
            var value = Environment.GetEnvironmentVariable(variableName);
            if (throwIfNotFound && string.IsNullOrWhiteSpace(value))
            {
                throw new Exception($"Variável de ambiente {variableName} não definida.");
            }
            return value;
        }
    }
}
