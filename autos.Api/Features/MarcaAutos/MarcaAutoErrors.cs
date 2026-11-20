using Common;

namespace autos.Api.Features.MarcaAutos;

public static class MarcaAutoErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound(
            "marca_auto_not_found",
            $"No existe una marca de auto con id '{id}'.");

    public static Error DuplicateName(string name) =>
        Error.Conflict(
            "marca_auto_duplicate_name",
            $"Ya existe una marca de auto con el nombre '{name}'.");
}
