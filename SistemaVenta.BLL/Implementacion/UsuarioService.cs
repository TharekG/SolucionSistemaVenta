using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using System.Net;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IGenericRepository<Usuario> _repositorio;
        private readonly IUtilidadesService _utilidadesService;
        private readonly ICorreoService _correoService;

        private const string CARPETA_FOTOS = "img/usuario";

        public UsuarioService(
            IGenericRepository<Usuario> repositorio,
            IUtilidadesService utilidadesService,
            ICorreoService correoService
            )
        {
            _repositorio = repositorio;
            _utilidadesService = utilidadesService;
            _correoService = correoService;
        }

        // ── Guardar foto localmente ───────────────────────────────────────
        private async Task<string> GuardarFotoLocal(Stream foto, string nombreFoto)
        {
            string carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", CARPETA_FOTOS);
            Directory.CreateDirectory(carpeta);
            string rutaArchivo = Path.Combine(carpeta, nombreFoto);
            using var stream = new FileStream(rutaArchivo, FileMode.Create);
            await foto.CopyToAsync(stream);
            return $"/{CARPETA_FOTOS}/{nombreFoto}";
        }

        private void EliminarFotoLocal(string nombreFoto)
        {
            if (string.IsNullOrWhiteSpace(nombreFoto)) return;
            string ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", CARPETA_FOTOS, nombreFoto);
            if (File.Exists(ruta)) File.Delete(ruta);
        }

        // ── Lista ─────────────────────────────────────────────────────────
        public async Task<List<Usuario>> Lista()
        {
            IQueryable<Usuario> query = await _repositorio.Consultar();
            return query.Include(r => r.IdRolNavigation).ToList();
        }

        // ── Crear ─────────────────────────────────────────────────────────
        public async Task<Usuario> Crear(Usuario entidad, Stream Foto = null, string NombreFoto = "", string UrlPlantillaCorreo = "")
        {
            Usuario usuario_existe = await _repositorio.Obtener(u => u.Correo == entidad.Correo);
            if (usuario_existe != null)
                throw new TaskCanceledException("El correo ya existe");

            try
            {
                string clave_generada = _utilidadesService.GenerarClave();
                entidad.Clave = _utilidadesService.ConvertirSha256(clave_generada);
                entidad.NombreFoto = NombreFoto;

                if (Foto != null)
                    entidad.UrlFoto = await GuardarFotoLocal(Foto, NombreFoto);

                Usuario usuario_creado = await _repositorio.Crear(entidad);

                if (usuario_creado.IdUsuario == 0)
                    throw new TaskCanceledException("No se pudo crear el usuario.");

                if (UrlPlantillaCorreo != "")
                {
                    UrlPlantillaCorreo = UrlPlantillaCorreo
                        .Replace("[correo]", usuario_creado.Correo)
                        .Replace("[clave]", clave_generada);

                    string htmlCorreo = "";
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlPlantillaCorreo);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using Stream dataStream = response.GetResponseStream();
                        var readerStream = response.CharacterSet == null
                            ? new StreamReader(dataStream)
                            : new StreamReader(dataStream, Encoding.GetEncoding(response.CharacterSet));
                        htmlCorreo = readerStream.ReadToEnd();
                        response.Close();
                        readerStream.Close();
                    }

                    if (htmlCorreo != "")
                        await _correoService.EnviarCorreo(usuario_creado.Correo, "Cuenta Creada", htmlCorreo);
                }

                IQueryable<Usuario> query = await _repositorio.Consultar(u => u.IdUsuario == usuario_creado.IdUsuario);
                return query.Include(r => r.IdRolNavigation).First();
            }
            catch { throw; }
        }

        // ── Editar ────────────────────────────────────────────────────────
        public async Task<Usuario> Editar(Usuario entidad, Stream Foto = null, string NombreFoto = "")
        {
            Usuario usuario_existe = await _repositorio.Obtener(u => u.Correo == entidad.Correo && u.IdUsuario != entidad.IdUsuario);
            if (usuario_existe != null)
                throw new TaskCanceledException("El correo ya existe");

            try
            {
                IQueryable<Usuario> queryUsuario = await _repositorio.Consultar(u => u.IdUsuario == entidad.IdUsuario);
                Usuario usuario_editar = queryUsuario.First();

                usuario_editar.Nombre = entidad.Nombre;
                usuario_editar.Correo = entidad.Correo;
                usuario_editar.Telefono = entidad.Telefono;
                usuario_editar.IdRol = entidad.IdRol;
                usuario_editar.EsActivo = entidad.EsActivo;

                if (Foto != null)
                {
                    // Eliminar foto anterior y guardar nueva
                    EliminarFotoLocal(usuario_editar.NombreFoto);
                    usuario_editar.NombreFoto = NombreFoto;
                    usuario_editar.UrlFoto = await GuardarFotoLocal(Foto, NombreFoto);
                }

                bool respuesta = await _repositorio.Editar(usuario_editar);
                if (!respuesta)
                    throw new TaskCanceledException("No se pudo modificar el usuario.");

                return queryUsuario.Include(r => r.IdRolNavigation).First();
            }
            catch { throw; }
        }

        // ── Eliminar ──────────────────────────────────────────────────────
        public async Task<bool> Eliminar(int IdUsuario)
        {
            try
            {
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.IdUsuario == IdUsuario);
                if (usuario_encontrado == null)
                    throw new TaskCanceledException("El usuario no existe.");

                string nombreFoto = usuario_encontrado.NombreFoto;
                bool respuesta = await _repositorio.Eliminar(usuario_encontrado);

                if (respuesta)
                    EliminarFotoLocal(nombreFoto);

                return true;
            }
            catch { throw; }
        }

        // ── Resto de métodos sin cambios ──────────────────────────────────
        public async Task<Usuario> ObtenerPorCredenciales(string correo, string clave)
        {
            string clave_encriptada = _utilidadesService.ConvertirSha256(clave);
            return await _repositorio.Obtener(u => u.Correo.Equals(correo) && u.Clave.Equals(clave_encriptada));
        }

        public async Task<Usuario> ObtenerPorId(int IdUsuario)
        {
            IQueryable<Usuario> query = await _repositorio.Consultar(u => u.IdUsuario == IdUsuario);
            return query.Include(r => r.IdRolNavigation).FirstOrDefault();
        }

        public async Task<bool> GuardarPerfil(Usuario entidad)
        {
            try
            {
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.IdUsuario == entidad.IdUsuario);
                if (usuario_encontrado == null)
                    throw new TaskCanceledException("El usuario no existe.");

                usuario_encontrado.Correo = entidad.Correo;
                usuario_encontrado.Telefono = entidad.Telefono;
                return await _repositorio.Editar(usuario_encontrado);
            }
            catch { throw; }
        }

        public async Task<bool> CambiarClave(int IdUsuario, string ClaveActual, string ClaveNueva)
        {
            try
            {
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.IdUsuario == IdUsuario);
                if (usuario_encontrado == null)
                    throw new TaskCanceledException("El usuario no existe.");

                if (usuario_encontrado.Clave != _utilidadesService.ConvertirSha256(ClaveActual))
                    throw new TaskCanceledException("La contraseña ingresada como actual no es correcta.");

                usuario_encontrado.Clave = _utilidadesService.ConvertirSha256(ClaveNueva);
                return await _repositorio.Editar(usuario_encontrado);
            }
            catch { throw; }
        }

        public async Task<bool> RestablecerClave(string Correo, string UrlPlantillaCorreo)
        {
            try
            {
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.Correo == Correo);
                if (usuario_encontrado == null)
                    throw new TaskCanceledException("No se encontro ningun usuario asociado al correo.");

                string clave_generada = _utilidadesService.GenerarClave();
                usuario_encontrado.Clave = _utilidadesService.ConvertirSha256(clave_generada);

                UrlPlantillaCorreo = UrlPlantillaCorreo.Replace("[clave]", clave_generada);

                string htmlCorreo = "";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlPlantillaCorreo);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using Stream dataStream = response.GetResponseStream();
                    var readerStream = response.CharacterSet == null
                        ? new StreamReader(dataStream)
                        : new StreamReader(dataStream, Encoding.GetEncoding(response.CharacterSet));
                    htmlCorreo = readerStream.ReadToEnd();
                    response.Close();
                    readerStream.Close();
                }

                if (htmlCorreo != "")
                    await _correoService.EnviarCorreo(Correo, "Contraseña Restablecida", htmlCorreo);

                return await _repositorio.Editar(usuario_encontrado);
            }
            catch { throw; }
        }
    }
}