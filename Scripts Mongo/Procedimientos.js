//Felipe Poveda

function RegistrarUsuario(nombres, apellidos, contrasena, correo) {

    // 1. Validar que no exista un usuario con el mismo correo
    const existente = db.Usuario.findOne({ correo: correo });
  
    if (existente) {
      print("ERROR: El correo ya está registrado.");
      return;
    }
  
    // 2. Insertar usuario nuevo
    const resultado = db.Usuario.insertOne({
      nombres: nombres,
      apellidos: apellidos,
      contraseña: contrasena,
      correo: correo,
      articulos: []  // obligatorio por tu schema
    });
  
    print("Usuario registrado con ID: " + resultado.insertedId);
  }
  
  
  
  function ConsultarUsuario(idUsuario) {
  
    const usuario = db.Usuario.findOne(
      { _id: ObjectId(idUsuario) }
    );
  
    if (!usuario) {
      print("No existe el usuario con ese ID.");
      return;
    }
  
    printjson(usuario);
  }
  
  
  
  function IniciarSesion(correo, contrasena) {
  
    const usuario = db.Usuario.findOne({
      correo: correo,
      contraseña: contrasena
    });
  
    if (!usuario) {
      print("xidUsuario = -1  // Credenciales incorrectas");
      return -1;
    }
  
    print("xidUsuario = " + usuario._id);
    return usuario._id;
  }
  
  
  
  function ConsultarArticulos(idUsuario) {
  
    return db.Usuario.aggregate([
      // 1. Filtramos el usuario
      {
        $match: { _id: ObjectId(idUsuario) }
      },
  
      // 2. "Abrimos" el array articulos
      {
        $unwind: "$articulos"
      },
  
      // 3. Ignorar articulos descartados
      {
        $match: {
          "articulos.descartado": false
        }
      },
  
      // 4. Hacemos un lookup a la colección Articulo
      {
        $lookup: {
          from: "Articulo",
          localField: "articulos.idArticulo",
          foreignField: "_id",
          as: "articulo"
        }
      },
  
      // 5. Convertir artículo de array a objeto
      {
        $unwind: "$articulo"
      },
  
      // 6. Proyección final
      {
        $project: {
          _id: 0,
          idArticulo: "$articulos.idArticulo",
          favorito: "$articulos.favorito",
          tema: "$articulo.tema",
          titular: "$articulo.titular",
          cuerpo: "$articulo.cuerpo",
          fecha: "$articulo.fecha",
          fuente: "$articulo.fuente"
        }
      }
    ]).toArray();
  }
  
  
  
  function toggle_favorito(idUsuario, idArticulo) {
  
    // 1. Buscar el artículo dentro del array del usuario
    const doc = db.Usuario.findOne(
      { 
        _id: ObjectId(idUsuario),
        "articulos.idArticulo": ObjectId(idArticulo)
      },
      {
        "articulos.$": 1
      }
    );
  
    // 2. Validaciones
    if (!doc) {
      print("El usuario no tiene este artículo asignado.");
      return;
    }
  
    const estadoActual = doc.articulos[0].favorito;
  
    // 3. Toggle (inversión del valor actual)
    db.Usuario.updateOne(
      { 
        _id: ObjectId(idUsuario),
        "articulos.idArticulo": ObjectId(idArticulo)
      },
      {
        $set: { "articulos.$.favorito": !estadoActual }
      }
    );
  
    print("Favorito cambiado a: " + (!estadoActual));
  }