using System;
using System.Collections.Generic;
using System.Linq;
using RolesDeUsuarioActividad2.Models;
using Microsoft.EntityFrameworkCore;

namespace RolesDeUsuarioActividad2.Repositories
{
    public class RolesDeUsuarioRepository<T> where T : class
    {
        public rolesdeusuarioContext Context { get; set; }
        public RolesDeUsuarioRepository(rolesdeusuarioContext context)
        {
            Context = context;
        }
        public virtual IEnumerable<T> ObtenerTodo()
        {
            return Context.Set<T>();
        }
        public T ObtenerPorId(object id)
        {
            return Context.Find<T>(id);
        }

        public virtual void Insertar(T entidad)
        {
            if (Validaciones(entidad))
            {
                Context.Add(entidad);
                Context.SaveChanges();
            }
        }
        public virtual void Editar(T entidad)
        {
            if (Validaciones(entidad))
            {
                Context.Update<T>(entidad);
                Context.SaveChanges();
            }
        }
        public virtual void Eliminar(T entidad)
        {
            Context.Remove<T>(entidad);
            Context.SaveChanges();
        }

        public virtual bool Validaciones(T entidad)
        {
            return true;
        }
    }

    public class MaestroRepository : RolesDeUsuarioRepository<Maestro>
    {
        public MaestroRepository(rolesdeusuarioContext context) : base(context) { }

        
        public Maestro ObtenerAlumnosPorMaestro(int idmaestro)
        {
            return Context.Maestro.Include(x => x.Alumno).FirstOrDefault(x => x.Id == idmaestro);
        }

        public Maestro ObtenerMaestroPorNoControl(int numcontrol)
        {
            return Context.Maestro.FirstOrDefault(x => x.NumControl == numcontrol);
        }

        public override bool Validaciones(Maestro maestro)
        {
            if (maestro.NumControl <= 0)
                throw new Exception("Ingrese el número de control.");
        
            if (string.IsNullOrWhiteSpace(maestro.Nombre))
                throw new Exception("Ingrese el nombre.");
            
            if (maestro.NumControl.ToString().Length > 5)
                throw new Exception("El número de control no debe exceder los 5 dígitos.");
            
            if (maestro.NumControl.ToString().Length < 5)
                throw new Exception("El número de control debe ser de 5 dígitos.");

            if (string.IsNullOrWhiteSpace(maestro.Clave))
                throw new Exception("Ingrese la contraseña.");

            return true;
        }
    }

    public class AlumnosRepository : RolesDeUsuarioRepository<Alumno>
    {
        public AlumnosRepository(rolesdeusuarioContext context) : base(context) { }

        public Alumno ObtenerAlumnoPorNoControl(string noControl)
        {
            return Context.Alumno.FirstOrDefault(x => x.NumControl.ToLower() == noControl.ToLower());
        }

        public override bool Validaciones(Alumno alumno)
        {
            if (string.IsNullOrEmpty(alumno.NumControl))
                throw new Exception("Ingrese el número de control del alumno.");
           
            if (string.IsNullOrEmpty(alumno.Nombre))
                throw new Exception("Ingrese el nombre del alumno.");
            
            if (alumno.NumControl.Length < 8)
                throw new Exception("El número de control debe ser de 8 caracteres.");
            
            if (alumno.NumControl.Length > 8)
                throw new Exception("El número de control no debe exceder los 8 caracteres.");

            if (alumno.IdMaestro.ToString() == null || alumno.IdMaestro <= 0)
                throw new Exception("Debe asignar un maestro.");

            return true;
        }
    }
}