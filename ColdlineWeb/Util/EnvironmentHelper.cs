using System;

namespace ColdlineWeb.Util
{
    /// <summary>
    /// Helper para acessar variáveis de ambiente do sistema.
    /// Centraliza a lógica e garante consistência no uso.
    /// </summary>
    public static class EnvironmentHelper
    {
        private const string FixedDepartmentId = "67f41c323a50bfa4e95bfe6d";
        private const string FixedUserTypeId   = "67f41bf13a50bfa4e95bfe69";

        /// <summary>
        /// Obtém o ID fixo do departamento a partir da variável de ambiente COLDLINE_FILTER_DEPARTMENT_ID.
        /// Se não existir, usa o valor padrão definido em código.
        /// </summary>
        public static string GetDepartmentIdIndustria()
        {
            var departmentId = Environment.GetEnvironmentVariable("COLDLINE_FILTER_DEPARTMENT_ID");
            return string.IsNullOrWhiteSpace(departmentId) ? FixedDepartmentId : departmentId;
        }

        /// <summary>
        /// Obtém o ID fixo do tipo de usuário a partir da variável de ambiente COLDLINE_FILTER_USER_TYPE_ID.
        /// Se não existir, usa o valor padrão definido em código.
        /// </summary>
        public static string GetUserTypeIdIndustria()
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
