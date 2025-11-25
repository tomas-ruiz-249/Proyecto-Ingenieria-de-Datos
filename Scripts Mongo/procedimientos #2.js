//Funcion generarNotificacion

function generarNotificacion(idResultadoFK, mensaje, tipo) {
    db.Notificacion.insertOne({
        mensaje: mensaje,
        tipo: tipo,
        leido: false,
        idResultadoFK: idResultadoFK
    });

    return { mensaje: "Notificación generada correctamente" };
}

//Funcion consultarNotificaciones

db.Notificacion.find({}) //Se encuentran todas las notificaciones sin importar los filtros

//Funcion eliminarNotificacion

function eliminarNotificacion(idNotificacion) {
    const notificacion = db.Notificacion.findOne({ _id: idNotificacion });

    if (!notificacion) {
        return { mensaje: "la notificacion no existe" };
    }

    db.Notificacion.deleteOne({ _id: idNotificacion });

    return { mensaje: "la notificacion fue eliminada correctamente" };
}

//Funcion filtrarNotificacion

function FiltrarNotificacion(idUsuarioP, tipoP, leidoP) {
    return db.Notificacion.aggregate([
        {
            $lookup: {
                from: "Resultado",
                localField: "idResultadoFK",
                foreignField: "_id",
                as: "resultado"
            }
        },
        { $unwind: "$resultado" },
        {
            $match: {
                ...(tipoP !== null ? { tipo: tipoP } : {}),
                ...(leidoP !== null ? { leido: leidoP } : {}),
                ...(idUsuarioP !== null ? { "resultado.idUsuarioFK": idUsuarioP } : {})
            }
        }
    ]);
}

//Funcion AsignarLecturaNotificacion

function AsignarLecturaNotificacion(idNotificacionP)
{
    const notificacion =
        db.Notificacion.findOne(
            { _id: idNotificacionP }
        );

    if (!notificacion)
    {
        return {
            mensaje: "La notificación no existe"
        };
    }

    const nuevoLeido =
        !notificacion.leido;

    db.Notificacion.updateOne(
        { _id: idNotificacionP },
        { $set: { leido: nuevoLeido } }
    );

    return {
        mensaje: "Estado de lectura actualizado",
        leido: nuevoLeido
    };
}


