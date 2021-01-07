using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RolesDeUsuarioActividad2.Models;
using RolesDeUsuarioActividad2.Repositories;
using RolesDeUsuarioActividad2.Helpers;
using RolesDeUsuarioActividad2.Models.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace RolesDeUsuarioActividad2.Controllers
{
    public class HomeController : Controller
    {
        [Authorize(Roles = "Maestro, Director")]
        public IActionResult Index(int numcontrol)
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult MaestroInicioSesion()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> MaestroInicioSesion(Maestro m)
        {
            rolesdeusuarioContext context = new rolesdeusuarioContext();
            MaestroRepository repository = new MaestroRepository(context);
            var maestro = repository.ObtenerMaestroPorNoControl(m.NumControl);
            try
            {
                if (maestro != null && maestro.Clave == HashHelper.GetHash(m.Clave))
                {
                    if (maestro.Activo == 1)
                    {
                        List<Claim> info = new List<Claim>();
                        info.Add(new Claim(ClaimTypes.Name, "Usuario" + maestro.Nombre));
                        info.Add(new Claim(ClaimTypes.Role, "Maestro"));
                        info.Add(new Claim("NumControl", maestro.NumControl.ToString()));
                        info.Add(new Claim("Nombre", maestro.Nombre));
                        info.Add(new Claim("Id", maestro.Id.ToString()));

                        var claimsidentity = new ClaimsIdentity(info, CookieAuthenticationDefaults.AuthenticationScheme);
                        var claimsprincipal = new ClaimsPrincipal(claimsidentity);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsprincipal,
                            new AuthenticationProperties { IsPersistent = true });
                        return RedirectToAction("Index", maestro.NumControl);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Su cuenta no esta activa.");
                        return View(m);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Número de control o contraseña estan incorrectas.");
                    return View(m);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(m);
            }
        }

        [AllowAnonymous]
        public IActionResult DirectorInicioSesion()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> DirectorInicioSesion(Director d)
        {
            rolesdeusuarioContext context = new rolesdeusuarioContext();
            RolesDeUsuarioRepository<Director> repository = new RolesDeUsuarioRepository<Director>(context);
            var director = context.Director.FirstOrDefault(x => x.NumControl == d.NumControl);
            try
            {
                if (director != null && director.Clave == HashHelper.GetHash(d.Clave))
                {
                    List<Claim> info = new List<Claim>();
                    info.Add(new Claim(ClaimTypes.Name, "Usuario" + director.Nombre));
                   
                    info.Add(new Claim(ClaimTypes.Role, "Director"));
                    
                    info.Add(new Claim("NumControl", director.Nombre.ToString()));
                    
                    info.Add(new Claim("Nombre", director.Nombre));

                    var claimsidentity = new ClaimsIdentity(info, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsprincipal = new ClaimsPrincipal(claimsidentity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsprincipal,
                        new AuthenticationProperties { IsPersistent = true });
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Número de control o contraseña estan incorrectas.");
                    return View(d);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(d);
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> CerrarSesion()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Director")]
        public IActionResult ListaDeMaestros()
        {
            rolesdeusuarioContext context = new rolesdeusuarioContext();
            MaestroRepository repository = new MaestroRepository(context);
            var maestros = repository.ObtenerTodo();
            return View(maestros);
        }

        [Authorize(Roles = "Director")]
        public IActionResult AltaMaestros()
        {
            return View();
        }

        [Authorize(Roles = "Director")]
        [HttpPost]
        public IActionResult AltaMaestros(Maestro m)
        {
            rolesdeusuarioContext context = new rolesdeusuarioContext();
            MaestroRepository repository = new MaestroRepository(context);

            try
            {
                var maestro = repository.ObtenerMaestroPorNoControl(m.NumControl);
                if (maestro == null)
                {
                    m.Activo = 1;
                    m.Clave = HashHelper.GetHash(m.Clave);
                    repository.Insertar(m);
                    return RedirectToAction("ListaDeMaestros");
                }
                else
                {
                    ModelState.AddModelError("", "El número de control que ingresó no es valido.");
                    return View(m);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(m);
            }
        }

        [Authorize(Roles = "Director")]
        [HttpPost]
        public IActionResult EstadoMaestro(Maestro m)
        {
            rolesdeusuarioContext context = new rolesdeusuarioContext();
            MaestroRepository repository = new MaestroRepository(context);
            var maestro = repository.ObtenerPorId(m.Id);
            if (maestro != null && maestro.Activo == 0)
            {
                maestro.Activo = 1;
                repository.Editar(maestro);
            }
            else
            {
                maestro.Activo = 0;
                repository.Editar(maestro);
            }
            return RedirectToAction("ListaDeMaestros");
        }

        [Authorize(Roles = "Director")]
        public IActionResult EditarMaestros(int id)
        {
            rolesdeusuarioContext context = new rolesdeusuarioContext();
            MaestroRepository repository = new MaestroRepository(context);
            var maestro = repository.ObtenerPorId(id);
            if (maestro != null)
            {
                return View(maestro);
            }
            return RedirectToAction("ListaDeMaestros");
        }

        [Authorize(Roles = "Director")]
        [HttpPost]
        public IActionResult EditarMaestros(Maestro m)
        {
            rolesdeusuarioContext context = new rolesdeusuarioContext();
            MaestroRepository repository = new MaestroRepository(context);
            var maestro = repository.ObtenerPorId(m.Id);
            try
            {
                if (maestro != null)
                {
                    maestro.Nombre = m.Nombre;
                    repository.Editar(maestro);
                }
                return RedirectToAction("ListaDeMaestros");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(maestro);
            }
        }

        [Authorize(Roles = "Director")]
        public IActionResult CambiarClaveMaestro(int id)
        {
            rolesdeusuarioContext context = new rolesdeusuarioContext();
            MaestroRepository repository = new MaestroRepository(context);
            var maestro = repository.ObtenerPorId(id);
            if (maestro == null)
            {
                return RedirectToAction("ListaDeMaestros");
            }
            return View(maestro);
        }

        [Authorize(Roles = "Director")]
        [HttpPost]
        public IActionResult CambiarClaveMaestro(Maestro m, string nuevaContra, string nuevaContraConfirm)
        {
            rolesdeusuarioContext context = new rolesdeusuarioContext();
            MaestroRepository repository = new MaestroRepository(context);
            var maestro = repository.ObtenerPorId(m.Id);
            try
            {

                if (maestro != null)
                {
                    if (nuevaContra != nuevaContraConfirm)
                    {
                        ModelState.AddModelError("", "Las contraseñas no coinciden.");
                        return View(maestro);
                    }
                    else if (maestro.Clave == HashHelper.GetHash(nuevaContra))
                    {
                        ModelState.AddModelError("", "La nueva contraseña no puede ser igual a la que desea cambiar.");
                        return View(maestro);
                    }
                    else
                    {
                        maestro.Clave = HashHelper.GetHash(nuevaContra);
                        repository.Editar(maestro);
                        return RedirectToAction("ListaDeMaestros");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "El maestro a editar no existe.");
                    return View(maestro);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(maestro);
            }
        }

        [Authorize(Roles = "Maestro, Director")]
        public IActionResult ListaDeAlumnos(int id)
        {
            rolesdeusuarioContext context = new rolesdeusuarioContext();
            MaestroRepository repository = new MaestroRepository(context);
            var maestro = repository.ObtenerAlumnosPorMaestro(id);
            if (maestro != null)
            {
                if (User.IsInRole("Maestro"))
                {
                    if (User.Claims.FirstOrDefault(x => x.Type == "Id").Value == maestro.Id.ToString())
                    {
                        return View(maestro);
                    }
                    else
                    {
                        return RedirectToAction("Denegado");
                    }
                }
                else
                {
                    return View(maestro);
                }
            }
            else
            {
                return RedirectToAction("ListaDeAlumnos");
            }
        }

        [Authorize(Roles = "Maestro, Director")]
        public IActionResult AgregarAlumno(int id)
        {
            rolesdeusuarioContext context = new rolesdeusuarioContext();
            MaestroRepository repository = new MaestroRepository(context);
            AlumnoViewModel viewModel = new AlumnoViewModel();
            viewModel.Maestro = repository.ObtenerPorId(id);
            if (viewModel.Maestro != null)
            {
                if (User.IsInRole("Maestro"))
                {
                    if (User.Claims.FirstOrDefault(x => x.Type == "Id").Value == viewModel.Maestro.Id.ToString())
                    {
                        return View(viewModel);
                    }
                    else
                    {
                        return RedirectToAction("Denegado");
                    }
                }
                else
                {
                    return View(viewModel);
                }
            }
            return View(viewModel);
        }

        [Authorize(Roles = "Maestro, Director")]
        [HttpPost]
        public IActionResult AgregarAlumno(AlumnoViewModel viewModel)
        {
            rolesdeusuarioContext context = new rolesdeusuarioContext();
            MaestroRepository maestroRepository = new MaestroRepository(context);
            AlumnosRepository alumnosRepository = new AlumnosRepository(context);
            try
            {
                if (context.Alumno.Any(x => x.NumControl == viewModel.Alumno.NumControl))
                {
                    ModelState.AddModelError("", "El número de control no esta disponible.");
                    return View(viewModel);
                }
                else
                {
                    var maestro = maestroRepository.ObtenerMaestroPorNoControl(viewModel.Maestro.NumControl).Id;
                    viewModel.Alumno.IdMaestro = maestro;
                    alumnosRepository.Insertar(viewModel.Alumno);
                    return RedirectToAction("ListaDeAlumnos", new { id = maestro });
                }

            }
            catch (Exception ex)
            {
                viewModel.Maestro = maestroRepository.ObtenerPorId(viewModel.Maestro.Id);
                viewModel.Maestros = maestroRepository.ObtenerTodo();
                ModelState.AddModelError("", ex.Message);
                return View(viewModel);
            }
        }

        [Authorize(Roles = "Maestro, Director")]
        public IActionResult EditarAlumno(int id)
        {
            rolesdeusuarioContext context = new rolesdeusuarioContext();
            MaestroRepository maestroRepository = new MaestroRepository(context);
            AlumnosRepository alumnosRepository = new AlumnosRepository(context);
            AlumnoViewModel viewModel = new AlumnoViewModel();
            viewModel.Alumno = alumnosRepository.ObtenerPorId(id);
            viewModel.Maestros = maestroRepository.ObtenerTodo();
            if (viewModel.Alumno != null)
            {
                viewModel.Maestro = maestroRepository.ObtenerPorId(viewModel.Alumno.IdMaestro);
                if (User.IsInRole("Maestro"))
                {
                    viewModel.Maestro = maestroRepository.ObtenerPorId(viewModel.Alumno.IdMaestro);
                    if (User.Claims.FirstOrDefault(x => x.Type == "NumControl").Value == viewModel.Maestro.NumControl.ToString())
                    {

                        return View(viewModel);
                    }
                }
                return View(viewModel);

            }
            else return RedirectToAction("Index");
        }

        [Authorize(Roles = "Maestro, Director")]
        [HttpPost]
        public IActionResult EditarAlumno(AlumnoViewModel viewModel)
        {
            rolesdeusuarioContext context = new rolesdeusuarioContext();
            MaestroRepository maestroRepository = new MaestroRepository(context);
            AlumnosRepository alumnosRepository = new AlumnosRepository(context);
            try
            {
                var alumno = alumnosRepository.ObtenerPorId(viewModel.Alumno.Id);
                if (alumno != null)
                {
                    alumno.Nombre = viewModel.Alumno.Nombre;
                    alumnosRepository.Editar(alumno);
                    return RedirectToAction("ListaDeAlumnos", new { id = alumno.IdMaestro });
                }
                else
                {
                    ModelState.AddModelError("", "El alumno a editar no existe.");
                    viewModel.Maestro = maestroRepository.ObtenerPorId(viewModel.Alumno.IdMaestro);
                    viewModel.Maestros = maestroRepository.ObtenerTodo();
                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                viewModel.Maestro = maestroRepository.ObtenerPorId(viewModel.Alumno.IdMaestro);
                viewModel.Maestros = maestroRepository.ObtenerTodo();
                return View(viewModel);
            }
        }

        [Authorize(Roles = "Maestro, Director")]
        [HttpPost]
        public IActionResult EliminarAlumno(Alumno a)
        {
            rolesdeusuarioContext context = new rolesdeusuarioContext();
            AlumnosRepository repository = new AlumnosRepository(context);
            var alumno = repository.ObtenerPorId(a.Id);
            if (alumno != null)
            {
                repository.Eliminar(alumno);
            }
            else
            {
                ModelState.AddModelError("", "El alumnó a eliminar no existe.");
            }
            return RedirectToAction("ListaDeAlumnos", new { id = alumno.IdMaestro });
        }

        [AllowAnonymous]
        public IActionResult Denegado()
        {
            return View();
        }
    }
}