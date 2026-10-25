namespace Casino_Dominio.InterfacesLN
{
    // Códigos de verificación de correo para crear la cuenta
    public interface IVerificacionCorreoLN
    {
        int MinutosValidez { get; }

        // Genera un código nuevo; devuelve null y el error si pidió otro hace muy poco
        (string? codigo, string error) Generar(string correo);

        // Revisa el código; si es correcto lo elimina para que no se reutilice
        (bool valido, string error) Verificar(string correo, string codigo);
    }
}
