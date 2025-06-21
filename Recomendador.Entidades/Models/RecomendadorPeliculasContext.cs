using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RecomendadorDePeliculas.Entidades.Models;

public interface IRecomendadorPeliculasContext
{
    DbSet<GeneroPelicula> GenerosPeliculas { get; }
    DbSet<UsuarioGenero> UsuarioGeneros { get; }
    DbSet<Usuario> Usuarios { get; }

    int SaveChanges();
}

public partial class RecomendadorPeliculasContext : DbContext, IRecomendadorPeliculasContext
{
    public RecomendadorPeliculasContext()
    {
    }

    public RecomendadorPeliculasContext(DbContextOptions<RecomendadorPeliculasContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Historial> Historials { get; set; }

    public virtual DbSet<Pelicula> Peliculas { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<GeneroPelicula> GenerosPeliculas { get; set; }

    public virtual DbSet<UsuarioGenero> UsuarioGeneros { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server= NICOLE;Database=RecomendadorPeliculas;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Historial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Historia__3213E83FF96EAB98");

            entity.ToTable("Historial");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FechaReseña).HasColumnType("datetime");
            entity.Property(e => e.IsCalificada).HasColumnName("isCalificada");
        });

        modelBuilder.Entity<Pelicula>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pelicula__3213E83F8873D13E");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Genres).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(255);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__usuario__3213E83FC05DB6C8");

            entity.ToTable("usuario");

            entity.HasIndex(e => e.Correo, "UQ__usuario__2A586E0B8DD0D346").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContraseniaHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("contrasenia_hash");
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.FechaDeNacimiento).HasColumnName("fecha_de_nacimiento");
            entity.Property(e => e.Genero)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("genero");
        });

        modelBuilder.Entity<GeneroPelicula>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(50);
            entity.ToTable("GeneroPelicula");
        });

        modelBuilder.Entity<UsuarioGenero>(entity =>
        {
            entity.ToTable("UsuarioGenero");
            entity.HasKey(e => new { e.UsuarioId, e.GeneroId });

            entity.HasOne(d => d.Usuario)
                .WithMany(p => p.UsuarioGeneros)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Genero)
                .WithMany(p => p.UsuarioGeneros)
                .HasForeignKey(d => d.GeneroId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
