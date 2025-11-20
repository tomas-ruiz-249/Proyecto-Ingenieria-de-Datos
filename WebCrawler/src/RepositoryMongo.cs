<<<<<<< HEAD

=======
>>>>>>> 58b0877 (mongo scraping)
using MongoDB.Bson;
using MongoDB.Driver;

class RepositoryMongo
{
    public RepositoryMongo(string connectionString)
    {
        if (connectionString == null)
        {
            throw new ArgumentNullException(nameof(connectionString), "You must provide a value for MONGODB_URL");
        }
        client = new MongoClient(connectionString);
        DB = client.GetDatabase("WebCrawlerCompass");
        Articulos = DB.GetCollection<BsonDocument>("Articulo");
        Usuarios = DB.GetCollection<BsonDocument>("Usuario");
        Notificaciones = DB.GetCollection<BsonDocument>("Notificacion");
        Connected = true;
    }
    public bool ChangePassword(string userId, string newPassword)
    {
        try
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(newPassword)) return false;
            if (!ObjectId.TryParse(userId, out ObjectId oid)) return false;
            var filter = Builders<BsonDocument>.Filter.Eq("_id", oid);
            var update = Builders<BsonDocument>.Update.Set("contraseña", newPassword);
            var result = Usuarios.UpdateOne(filter, update);
            return result.ModifiedCount > 0;
        }
        catch
        {
            return false;
        }
    }

    public void DeleteNotification(string notifId)
    {
        if (string.IsNullOrEmpty(notifId)) return;
        if (!ObjectId.TryParse(notifId, out ObjectId oid)) return;
        var filter = Builders<BsonDocument>.Filter.Eq("_id", oid);
        Notificaciones.DeleteOne(filter);
    }

    public bool DeleteResults(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId)) return false;
            // find user
            var userFilter = ObjectId.TryParse(userId, out ObjectId uoid)
                ? Builders<BsonDocument>.Filter.Eq("_id", uoid)
                : Builders<BsonDocument>.Filter.Eq("_id", userId);
            var userDoc = Usuarios.Find(userFilter).FirstOrDefault();
            if (userDoc == null) return false;
<<<<<<< HEAD
            // If user has Resultado subdocument, remove related articles and notifications
            var resultado = userDoc.Contains("Resultado") ? userDoc["Resultado"] : null;
            if (resultado != null && resultado.IsBsonDocument)
            {
                var resId = resultado.AsBsonDocument.Contains("_id") ? resultado["_id"].ToString() : null;
                if (!string.IsNullOrEmpty(resId) && ObjectId.TryParse(resId, out ObjectId resid))
                {
                    // delete articles with idResultado == resid
                    var artFilter = Builders<BsonDocument>.Filter.Eq("idResultado", resid.ToString());
                    Articulos.DeleteMany(artFilter);
                    // delete notifications referencing this result
                    var notifFilter = Builders<BsonDocument>.Filter.Eq("idResultado", resid.ToString());
                    Notificaciones.DeleteMany(notifFilter);
                }
            }
            // unset Resultado and clear Articulos reference array
            var update = Builders<BsonDocument>.Update.Unset("Resultado").Unset("articulos");
=======
            // Remove all articles and notifications for all resultados in the array
            if (userDoc.Contains("resultados") && userDoc["resultados"].IsBsonArray)
            {
                var resultadosArray = userDoc["resultados"].AsBsonArray;
                foreach (var resultado in resultadosArray)
                {
                    if (resultado.IsBsonDocument && resultado.AsBsonDocument.Contains("_id"))
                    {
                        var resIdVal = resultado["_id"];
                        if (!resIdVal.IsBsonNull)
                        {
                            var resIdStr = resIdVal.ToString();
                            if (ObjectId.TryParse(resIdStr, out ObjectId resid))
                            {
                                var artFilter = Builders<BsonDocument>.Filter.Eq("idResultado", resid);
                                Articulos.DeleteMany(artFilter);
                                var notifFilter = Builders<BsonDocument>.Filter.Eq("idResultado", resid);
                                Notificaciones.DeleteMany(notifFilter);
                            }
                        }
                    }
                }
            }
            // clear resultados and articulos arrays
            var update = Builders<BsonDocument>.Update.Set("resultados", new BsonArray()).Set("articulos", new BsonArray());
>>>>>>> 58b0877 (mongo scraping)
            var res = Usuarios.UpdateOne(userFilter, update);
            return res.ModifiedCount > 0;
        }
        catch
        {
            return false;
        }
    }

    public bool DeleteUser(string userId)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", userId);
        var result = Usuarios.DeleteOne(filter);
        return result.DeletedCount > 0;
    }

    public bool DiscardArticles(string userId, List<string> discardIds)
    {
        try
        {
            if (string.IsNullOrEmpty(userId) || discardIds == null) return false;
            var userFilter = ObjectId.TryParse(userId, out ObjectId uoid)
                ? Builders<BsonDocument>.Filter.Eq("_id", uoid)
                : Builders<BsonDocument>.Filter.Eq("_id", userId);
            var user = Usuarios.Find(userFilter).FirstOrDefault();
            if (user == null) return false;
            // Update each article ref in the user's articulos array
            foreach (var aid in discardIds)
            {
                if (!ObjectId.TryParse(aid, out ObjectId aobj)) continue;
                var arrayFilter = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("_id", uoid),
<<<<<<< HEAD
                    Builders<BsonDocument>.Filter.Eq("articulos.idArticulo", aid)
=======
                    Builders<BsonDocument>.Filter.Eq("articulos.idArticulo", aobj)
>>>>>>> 58b0877 (mongo scraping)
                );
                var update = Builders<BsonDocument>.Update.Set("articulos.$.descartado", true);
                Usuarios.UpdateOne(arrayFilter, update);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool EditEmail(string userId, string email)
    {
        try
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email)) return false;
            if (!ObjectId.TryParse(userId, out ObjectId oid)) return false;
            var filter = Builders<BsonDocument>.Filter.Eq("_id", oid);
            var update = Builders<BsonDocument>.Update.Set("correo", email);
            var result = Usuarios.UpdateOne(filter, update);
            return result.ModifiedCount > 0;
        }
        catch
        {
            return false;
        }
    }

    public bool EditUser(string userId, string name, string lastName)
    {
        try
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(lastName)) return false;
            if (!ObjectId.TryParse(userId, out ObjectId oid)) return false;
            var filter = Builders<BsonDocument>.Filter.Eq("_id", oid);
            var update = Builders<BsonDocument>.Update.Set("nombres", name).Set("apellidos", lastName);
            var result = Usuarios.UpdateOne(filter, update);
            return result.ModifiedCount > 0;
        }
        catch
        {
            return false;
        }
    }

    public List<ArticleSource> FilterArticles(string userId, string? titular, string? palabrasClave, string? tema, string? nombreFuente, string? fecha1, string? fecha2)
    {
        var result = new List<ArticleSource>();
        try
        {
            if (string.IsNullOrEmpty(userId)) return result;
            // find user and their article refs
            var userFilter = ObjectId.TryParse(userId, out ObjectId uoid)
                ? Builders<BsonDocument>.Filter.Eq("_id", uoid)
                : Builders<BsonDocument>.Filter.Eq("_id", userId);
            var userDoc = Usuarios.Find(userFilter).FirstOrDefault();
            if (userDoc == null) return result;
            var articRefs = new List<string>();
            if (userDoc.Contains("articulos") && userDoc["articulos"].IsBsonArray)
            {
                foreach (var ar in userDoc["articulos"].AsBsonArray)
                {
                    if (ar.AsBsonDocument.Contains("idArticulo"))
                    {
                        var val = ar.AsBsonDocument.GetValue("idArticulo", BsonNull.Value);
                        var idStr = val.IsBsonNull ? string.Empty : val.ToString();
                        if (!string.IsNullOrEmpty(idStr)) articRefs.Add(idStr);
                    }
                }
            }
            if (articRefs.Count == 0) return result;
            var filter = Builders<BsonDocument>.Filter.In("_id", articRefs.Select(id => ObjectId.TryParse(id, out var o) ? (BsonValue)new ObjectId(id) : BsonValue.Create(id)).ToList());
            var docs = Articulos.Find(filter).ToList();
            foreach (var d in docs)
            {
                var temaVal = d.Contains("tema") ? d["tema"].AsString : string.Empty;
                var titularVal = d.Contains("titular") ? d["titular"].AsString : string.Empty;
                // simple text filters
                if (!string.IsNullOrEmpty(titular) && !titularVal.Contains(titular, StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.IsNullOrEmpty(tema) && !temaVal.Equals(tema, StringComparison.OrdinalIgnoreCase)) continue;
                // build Articulo and Fuente
                var cuerpo = d.Contains("cuerpo") ? d["cuerpo"].AsString : string.Empty;
                var fecha = d.Contains("fecha") && d["fecha"].IsValidDateTime ? d["fecha"].ToUniversalTime().ToString("o") : string.Empty;
                var idResultado = d.Contains("idResultado") ? d["idResultado"].ToString() : "";
                var articulo = new Articulo(temaVal, titularVal, string.Empty, cuerpo, fecha, -1, false);
<<<<<<< HEAD
=======
                // set MongoId and Id from _id
                if (d.Contains("_id"))
                {
                    var idVal = d["_id"];
                    articulo.MongoId = idVal.ToString();
                    int idInt;
                    if (int.TryParse(idVal.ToString(), out idInt))
                        articulo.Id = idInt;
                }
>>>>>>> 58b0877 (mongo scraping)
                // fuente nested
                Fuente fuente = new Fuente(string.Empty, string.Empty, string.Empty);
                if (d.Contains("fuente") && d["fuente"].IsBsonDocument)
                {
                    var f = d["fuente"].AsBsonDocument;
                    var url = f.Contains("url") ? f["url"].AsString : string.Empty;
                    var tipoVal = f.Contains("tipo") ? f["tipo"].AsString : string.Empty;
                    var nombreVal = f.Contains("nombre") ? f["nombre"].AsString : string.Empty;
                    fuente = new Fuente(url, tipoVal, nombreVal);
                }
                result.Add(new ArticleSource(articulo, fuente));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return result;
    }

    public List<Notificacion> FilterNotifications(string idUsuario, int? tipo, bool? leido)
    {
        var list = new List<Notificacion>();
        try
        {
            if (string.IsNullOrEmpty(idUsuario)) return list;
<<<<<<< HEAD
            // find user's resultado id if any
=======
            // find user's result ids if any
>>>>>>> 58b0877 (mongo scraping)
            var userFilter = ObjectId.TryParse(idUsuario, out ObjectId uoid)
                ? Builders<BsonDocument>.Filter.Eq("_id", uoid)
                : Builders<BsonDocument>.Filter.Eq("_id", idUsuario);
            var userDoc = Usuarios.Find(userFilter).FirstOrDefault();
            if (userDoc == null) return list;
<<<<<<< HEAD
            string? resId = null;
            if (userDoc.Contains("Resultado") && userDoc["Resultado"].IsBsonDocument)
            {
                var rd = userDoc["Resultado"].AsBsonDocument;
                var val = rd.GetValue("_id", BsonNull.Value);
                resId = val.IsBsonNull ? null : val.ToString();
            }
            var filter = resId != null ? Builders<BsonDocument>.Filter.Eq("idResultado", resId) : Builders<BsonDocument>.Filter.Empty;
=======
            var resultIds = new List<ObjectId>();
            if (userDoc.Contains("resultados") && userDoc["resultados"].IsBsonArray)
            {
                foreach (var resultado in userDoc["resultados"].AsBsonArray)
                {
                    if (resultado.IsBsonDocument && resultado.AsBsonDocument.Contains("_id"))
                    {
                        var val = resultado["_id"];
                        if (!val.IsBsonNull && val.BsonType == BsonType.ObjectId)
                            resultIds.Add(val.AsObjectId);
                        else if (!val.IsBsonNull && ObjectId.TryParse(val.ToString(), out var parsed))
                            resultIds.Add(parsed);
                    }
                }
            }
            var filter = resultIds.Count > 0 ? Builders<BsonDocument>.Filter.In("idResultado", resultIds) : Builders<BsonDocument>.Filter.Empty;
>>>>>>> 58b0877 (mongo scraping)
            if (tipo.HasValue) filter &= Builders<BsonDocument>.Filter.Eq("tipo", tipo.Value);
            if (leido.HasValue) filter &= Builders<BsonDocument>.Filter.Eq("leido", leido.Value);
            var docs = Notificaciones.Find(filter).ToList();
            foreach (var d in docs)
            {
                var id = d.Contains("_id") ? d["_id"].ToString() : "";
                var mensaje = d.Contains("mensaje") ? d["mensaje"].AsString : string.Empty;
                var tipoVal = d.Contains("tipo") ? d["tipo"].AsInt32 : 0;
                var leidoVal = d.Contains("leido") ? d["leido"].AsBoolean : false;
<<<<<<< HEAD
                var idRes = d.Contains("idResultado") ? d["idResultado"].ToString() : string.Empty;
                int idResInt = -1;
                int.TryParse(idRes, out idResInt);
                list.Add(new Notificacion(-1, mensaje, tipoVal, leidoVal, idResInt));
=======
                // idResultado is stored as ObjectId in Mongo; we return -1 for SQL-style IdResultado
                list.Add(new Notificacion(-1, mensaje, tipoVal, leidoVal, -1));
>>>>>>> 58b0877 (mongo scraping)
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return list;
    }

    public bool GenerateNotification(string idResult, string message, int type)
    {
        try
        {
<<<<<<< HEAD
=======
            ObjectId objIdResult;
            if (!ObjectId.TryParse(idResult, out objIdResult))
            {
                objIdResult = ObjectId.GenerateNewId();
            }
>>>>>>> 58b0877 (mongo scraping)
            var doc = new BsonDocument
            {
                { "mensaje", message },
                { "tipo", type },
                { "leido", false },
<<<<<<< HEAD
                { "idResultado", idResult }
=======
                { "idResultado", objIdResult }
>>>>>>> 58b0877 (mongo scraping)
            };
            Notificaciones.InsertOne(doc);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public List<ArticleSource> GetArticlesAndSources(string userId)
    {
        var result = new List<ArticleSource>();
        try
        {
            if (string.IsNullOrEmpty(userId)) return result;
            var userFilter = ObjectId.TryParse(userId, out ObjectId uoid)
                ? Builders<BsonDocument>.Filter.Eq("_id", uoid)
                : Builders<BsonDocument>.Filter.Eq("_id", userId);
            var userDoc = Usuarios.Find(userFilter).FirstOrDefault();
            if (userDoc == null) return result;
            var articRefs = new List<string>();
            if (userDoc.Contains("articulos") && userDoc["articulos"].IsBsonArray)
            {
                foreach (var ar in userDoc["articulos"].AsBsonArray)
                {
                    if (ar.AsBsonDocument.Contains("idArticulo"))
                    {
                        var val = ar.AsBsonDocument.GetValue("idArticulo", BsonNull.Value);
                        var idStr = val.IsBsonNull ? string.Empty : val.ToString();
                        if (!string.IsNullOrEmpty(idStr)) articRefs.Add(idStr);
                    }
                }
            }
            if (articRefs.Count == 0) return result;
            var filter = Builders<BsonDocument>.Filter.In("_id", articRefs.Select(id => ObjectId.TryParse(id, out var o) ? (BsonValue)new ObjectId(id) : BsonValue.Create(id)).ToList());
            var docs = Articulos.Find(filter).ToList();
            foreach (var d in docs)
            {
                var temaVal = d.Contains("tema") ? d["tema"].AsString : string.Empty;
                var titularVal = d.Contains("titular") ? d["titular"].AsString : string.Empty;
                var cuerpo = d.Contains("cuerpo") ? d["cuerpo"].AsString : string.Empty;
                var fecha = d.Contains("fecha") && d["fecha"].IsValidDateTime ? d["fecha"].ToUniversalTime().ToString("o") : string.Empty;
                var articulo = new Articulo(temaVal, titularVal, string.Empty, cuerpo, fecha, -1, false);
<<<<<<< HEAD
=======
                // set MongoId from _id (do not set Id, which is for SQL)
                if (d.Contains("_id"))
                {
                    var idVal = d["_id"];
                    articulo.MongoId = idVal?.ToString() ?? string.Empty;
                }
>>>>>>> 58b0877 (mongo scraping)
                Fuente fuente = new Fuente(string.Empty, string.Empty, string.Empty);
                if (d.Contains("fuente") && d["fuente"].IsBsonDocument)
                {
                    var f = d["fuente"].AsBsonDocument;
                    var url = f.Contains("url") ? f["url"].AsString : string.Empty;
                    var tipoVal = f.Contains("tipo") ? f["tipo"].AsString : string.Empty;
                    var nombreVal = f.Contains("nombre") ? f["nombre"].AsString : string.Empty;
                    fuente = new Fuente(url, tipoVal, nombreVal);
                }
                result.Add(new ArticleSource(articulo, fuente));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return result;
    }

<<<<<<< HEAD
    public Result? GetLastResult(string id)
=======
    public Result? GetResult(string id)
>>>>>>> 58b0877 (mongo scraping)
    {
        try
        {
            if (string.IsNullOrEmpty(id)) return default;
<<<<<<< HEAD
            var userFilter = ObjectId.TryParse(id, out ObjectId uoid)
                ? Builders<BsonDocument>.Filter.Eq("_id", uoid)
                : Builders<BsonDocument>.Filter.Eq("_id", id);
            var userDoc = Usuarios.Find(userFilter).FirstOrDefault();
            if (userDoc == null) return default;
            if (userDoc.Contains("Resultado") && userDoc["Resultado"].IsBsonDocument)
            {
                var rd = userDoc["Resultado"].AsBsonDocument;
                var estado = rd.Contains("estado") ? rd["estado"].AsInt32 : -1;
                var fecha = rd.Contains("fechaExtraccion") ? rd["fechaExtraccion"].ToUniversalTime().ToString("o") : string.Empty;
                return new Result(-1, -1, estado, fecha);
            }
=======

            var userFilter = ObjectId.TryParse(id, out ObjectId uoid)
                ? Builders<BsonDocument>.Filter.Eq("_id", uoid)
                : Builders<BsonDocument>.Filter.Eq("_id", id);

            var userDoc = Usuarios.Find(userFilter).FirstOrDefault();
            if (userDoc == null) return default;

            if (userDoc.Contains("resultados") && userDoc["resultados"].IsBsonArray)
            {
                var resultadosArray = userDoc["resultados"].AsBsonArray;
                foreach (var resultado in resultadosArray)
                {
                    if (resultado.IsBsonDocument && resultado["_id"].ToString() == id)
                    {
                        var rd = resultado.AsBsonDocument;
                        var estado = rd.Contains("estado") ? rd["estado"].AsInt32 : -1;
                        var fecha = rd.Contains("fechaExtraccion") ? rd["fechaExtraccion"].ToUniversalTime().ToString("o") : string.Empty;
                        var mongoId = rd.Contains("_id") ? rd["_id"].ToString() : string.Empty;
                        var res = new Result(-1, -1, estado, fecha);
                        res.MongoId = mongoId;
                        return res;
                    }
                }
            }

>>>>>>> 58b0877 (mongo scraping)
            return default;
        }
        catch
        {
            return default;
        }
    }

    public string GetLastResultId()
    {
        try
        {
<<<<<<< HEAD
            // Return last Resultado id from any user if present (best-effort)
            var doc = Usuarios.Find(Builders<BsonDocument>.Filter.Exists("Resultado")).Sort(Builders<BsonDocument>.Sort.Descending("Resultado.fechaExtraccion")).FirstOrDefault();
            if (doc != null && doc.Contains("Resultado") && doc["Resultado"].IsBsonDocument)
            {
                var rd = doc["Resultado"].AsBsonDocument;
                var val = rd.GetValue("_id", BsonNull.Value);
                return val.IsBsonNull ? string.Empty : val.ToString()!;
=======
            // Find the user with the most recent result in the 'resultados' array
            var userDoc = Usuarios.Find(Builders<BsonDocument>.Filter.Exists("resultados")).Sort(Builders<BsonDocument>.Sort.Descending("resultados.fechaExtraccion")).FirstOrDefault();
            if (userDoc != null && userDoc.Contains("resultados") && userDoc["resultados"].IsBsonArray)
            {
                var resultadosArray = userDoc["resultados"].AsBsonArray;
                if (resultadosArray.Count > 0)
                {
                    // Find the result with the latest fechaExtraccion
                    BsonDocument? latest = null;
                    DateTime latestFecha = DateTime.MinValue;
                    foreach (var res in resultadosArray)
                    {
                        if (res.IsBsonDocument && res.AsBsonDocument.Contains("fechaExtraccion"))
                        {
                            var fechaVal = res["fechaExtraccion"];
                            DateTime fecha;
                            if (fechaVal.IsValidDateTime)
                            {
                                fecha = fechaVal.ToUniversalTime();
                                if (fecha > latestFecha)
                                {
                                    latestFecha = fecha;
                                    latest = res.AsBsonDocument;
                                }
                            }
                        }
                    }
                    if (latest != null && latest.Contains("_id"))
                    {
                        var val = latest["_id"];
                        return val.IsBsonNull ? string.Empty : val.ToString();
                    }
                }
>>>>>>> 58b0877 (mongo scraping)
            }
            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public List<Notificacion> GetNotifications(string idUsuario)
    {
        var list = new List<Notificacion>();
        try
        {
            if (string.IsNullOrEmpty(idUsuario)) return list;
            var userFilter = ObjectId.TryParse(idUsuario, out ObjectId uoid)
                ? Builders<BsonDocument>.Filter.Eq("_id", uoid)
                : Builders<BsonDocument>.Filter.Eq("_id", idUsuario);
            var userDoc = Usuarios.Find(userFilter).FirstOrDefault();
            if (userDoc == null) return list;
<<<<<<< HEAD
            string? resId = null;
            if (userDoc.Contains("Resultado") && userDoc["Resultado"].IsBsonDocument)
            {
                var rd = userDoc["Resultado"].AsBsonDocument;
                var val = rd.GetValue("_id", BsonNull.Value);
                resId = val.IsBsonNull ? null : val.ToString();
            }
            var filter = resId != null ? Builders<BsonDocument>.Filter.Eq("idResultado", resId) : Builders<BsonDocument>.Filter.Empty;
=======
            // Collect all result ids for this user
            var resultIds = new List<ObjectId>();
            if (userDoc.Contains("resultados") && userDoc["resultados"].IsBsonArray)
            {
                foreach (var resultado in userDoc["resultados"].AsBsonArray)
                {
                    if (resultado.IsBsonDocument && resultado.AsBsonDocument.Contains("_id"))
                    {
                        var val = resultado["_id"];
                        if (!val.IsBsonNull && val.BsonType == BsonType.ObjectId)
                            resultIds.Add(val.AsObjectId);
                        else if (!val.IsBsonNull && ObjectId.TryParse(val.ToString(), out var parsed))
                            resultIds.Add(parsed);
                    }
                }
            }
            var filter = resultIds.Count > 0 ? Builders<BsonDocument>.Filter.In("idResultado", resultIds) : Builders<BsonDocument>.Filter.Empty;
>>>>>>> 58b0877 (mongo scraping)
            var docs = Notificaciones.Find(filter).ToList();
            foreach (var d in docs)
            {
                var mensaje = d.Contains("mensaje") ? d["mensaje"].AsString : string.Empty;
                var tipoVal = d.Contains("tipo") ? d["tipo"].AsInt32 : 0;
                var leidoVal = d.Contains("leido") ? d["leido"].AsBoolean : false;
<<<<<<< HEAD
                var idRes = d.Contains("idResultado") ? d["idResultado"].ToString() : string.Empty;
                int idResInt = -1;
                int.TryParse(idRes, out idResInt);
                list.Add(new Notificacion(-1, mensaje, tipoVal, leidoVal, idResInt));
=======
                list.Add(new Notificacion(-1, mensaje, tipoVal, leidoVal, -1));
>>>>>>> 58b0877 (mongo scraping)
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return list;
    }

    public List<Result> GetResults(string userId)
    {
        var list = new List<Result>();
        try
        {
            if (string.IsNullOrEmpty(userId)) return list;
            var userFilter = ObjectId.TryParse(userId, out ObjectId uoid)
                ? Builders<BsonDocument>.Filter.Eq("_id", uoid)
                : Builders<BsonDocument>.Filter.Eq("_id", userId);
            var userDoc = Usuarios.Find(userFilter).FirstOrDefault();
            if (userDoc == null) return list;
<<<<<<< HEAD
            if (userDoc.Contains("Resultado") && userDoc["Resultado"].IsBsonDocument)
            {
                var rd = userDoc["Resultado"].AsBsonDocument;
                var estado = rd.Contains("estado") ? rd["estado"].AsInt32 : -1;
                var fecha = rd.Contains("fechaExtraccion") ? rd["fechaExtraccion"].ToUniversalTime().ToString("o") : string.Empty;
                list.Add(new Result(-1, -1, estado, fecha));
=======
            if (userDoc.Contains("resultados") && userDoc["resultados"].IsBsonArray)
            {
                var resultadosArray = userDoc["resultados"].AsBsonArray;
                foreach (var resultado in resultadosArray)
                {
                    if (resultado.IsBsonDocument)
                    {
                        var rd = resultado.AsBsonDocument;
                        var estado = rd.Contains("estado") ? rd["estado"].AsInt32 : -1;
                        var fecha = rd.Contains("fechaExtraccion") ? rd["fechaExtraccion"].ToUniversalTime().ToString("o") : string.Empty;
                        var mongoId = rd.Contains("_id") ? rd["_id"].ToString() : string.Empty;
                        var cantidad = rd.Contains("numArticulos") ? rd["numArticulos"].ToInt32() : 0;
                        var res = new Result(-1, -1, estado, fecha)
                        {
                            MongoId = mongoId,
                            MongoIdUsuario = userId,
                            Cantidad = cantidad
                        };
                        list.Add(res);
                    }
                }
>>>>>>> 58b0877 (mongo scraping)
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return list;
    }

    public Usuario GetUser(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id)) return new Usuario(-1, "", "", "");
            var filter = ObjectId.TryParse(id, out ObjectId oid)
                ? Builders<BsonDocument>.Filter.Eq("_id", oid)
                : Builders<BsonDocument>.Filter.Eq("_id", id);
            var doc = Usuarios.Find(filter).FirstOrDefault();
            if (doc == null) return new Usuario(-1, "", "", "");
            var nombres = doc.Contains("nombres") ? doc["nombres"].AsString : string.Empty;
            var apellidos = doc.Contains("apellidos") ? doc["apellidos"].AsString : string.Empty;
            var correo = doc.Contains("correo") ? doc["correo"].AsString : string.Empty;
            var u = new Usuario(-1, nombres, apellidos, correo);
            // attach Mongo ObjectId string when available so callers can access it
            var idVal = doc.GetValue("_id", BsonNull.Value);
            u.MongoId = idVal.IsBsonNull ? string.Empty : idVal.ToString()!;
            return u;
        }
        catch
        {
            return new Usuario(-1, "", "", "");
        }
    }

    public bool RegisterScraping(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId)) return false;
            var filter = ObjectId.TryParse(userId, out ObjectId oid)
                ? Builders<BsonDocument>.Filter.Eq("_id", oid)
                : Builders<BsonDocument>.Filter.Eq("_id", userId);
<<<<<<< HEAD
            var newResultId = ObjectId.GenerateNewId().ToString();
            var resultadoDoc = new BsonDocument
            {
                { "_id", new ObjectId(newResultId) },
                { "fechaExtraccion", DateTime.UtcNow },
                { "estado", 2 }
            };
            var update = Builders<BsonDocument>.Update.Set("Resultado", resultadoDoc);
            Usuarios.UpdateOne(filter, update);
            // create notification
=======

            var newResultObjId = ObjectId.GenerateNewId();
            var resultadoDoc = new BsonDocument
            {
                { "_id", newResultObjId },
                { "fechaExtraccion", DateTime.UtcNow },
                { "estado", 2 }
            };

            var update = Builders<BsonDocument>.Update.Push("resultados", resultadoDoc);
            Usuarios.UpdateOne(filter, update);

            // create notification with ObjectId
>>>>>>> 58b0877 (mongo scraping)
            var notif = new BsonDocument
            {
                { "mensaje", "Scraping iniciado..." },
                { "tipo", 2 },
                { "leido", false },
<<<<<<< HEAD
                { "idResultado", newResultId }
            };
            Notificaciones.InsertOne(notif);
=======
                { "idResultado", newResultObjId }
            };
            Notificaciones.InsertOne(notif);

>>>>>>> 58b0877 (mongo scraping)
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool RegisterUser(Usuario u)
    {
        try
        {
            if (u == null) return false;
            // check existing email
            var exists = Usuarios.Find(Builders<BsonDocument>.Filter.Eq("correo", u.Correo)).Any();
            if (exists) return false;
            var doc = new BsonDocument
            {
                { "nombres", u.Nombres ?? string.Empty },
                { "apellidos", u.Apellidos ?? string.Empty },
                { "correo", u.Correo ?? string.Empty },
                { "contraseña", u.Contraseña ?? string.Empty },
<<<<<<< HEAD
                { "articulos", new BsonArray() }
=======
                // articulos: empty array, each entry must have idUsuario, idArticulo, descartado, favorito
                { "articulos", new BsonArray() },
                // resultados: empty array
                { "resultados", new BsonArray() }
>>>>>>> 58b0877 (mongo scraping)
            };
            Usuarios.InsertOne(doc);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public bool SetResultFinished(string resultId, int articleCount)
    {
        try
        {
            if (string.IsNullOrEmpty(resultId)) return false;
<<<<<<< HEAD
            // find user that has this resultado and update estado to 0 and numArticulos if present
            var userFilter = Builders<BsonDocument>.Filter.Eq("Resultado._id", new ObjectId(resultId));
            var update = Builders<BsonDocument>.Update.Set("Resultado.estado", 0).Set("Resultado.numArticulos", articleCount);
            var res = Usuarios.UpdateOne(userFilter, update);
            return res.ModifiedCount > 0;
        }
        catch
        {
=======
            if (!ObjectId.TryParse(resultId, out ObjectId resObjId)) return false;
            // Find the user with this result in resultados array
            var userFilter = Builders<BsonDocument>.Filter.ElemMatch("resultados", Builders<BsonDocument>.Filter.Eq("_id", resObjId));
            // Update the correct element in the resultados array
            var arrayFilters = new List<ArrayFilterDefinition<BsonDocument>>
            {
                new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("elem._id", resObjId))
            };
            var update = Builders<BsonDocument>.Update
                .Set("resultados.$[elem].estado", 0)
                .Set("resultados.$[elem].numArticulos", articleCount);
            var res = Usuarios.UpdateOne(userFilter, update, new UpdateOptions { ArrayFilters = arrayFilters });
            return res.ModifiedCount > 0;
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
>>>>>>> 58b0877 (mongo scraping)
            return false;
        }
    }

    public bool StoreArticleWithSource(Articulo a, Fuente f, string idResultado)
    {
        try
        {
            if (a == null || f == null || string.IsNullOrEmpty(idResultado)) return false;
<<<<<<< HEAD
=======
            // ensure idResultado is an ObjectId
            ObjectId objIdResult;
            if (!ObjectId.TryParse(idResultado, out objIdResult))
            {
                objIdResult = ObjectId.GenerateNewId();
            }
>>>>>>> 58b0877 (mongo scraping)
            // check duplicate by titular + fuente.url
            var existing = Articulos.Find(Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("titular", a.Titular ?? string.Empty),
                Builders<BsonDocument>.Filter.Eq("fuente.url", f.Url ?? string.Empty)
            )).FirstOrDefault();
<<<<<<< HEAD
            string artId;
            if (existing != null)
            {
                var val = existing.GetValue("_id", BsonNull.Value);
                artId = val.IsBsonNull ? ObjectId.GenerateNewId().ToString() : val.ToString()!;
            }
            else
            {
                var doc = new BsonDocument
                {
                    { "tema", a.Tema ?? string.Empty },
                    { "titular", a.Titular ?? string.Empty },
                    { "subtitulo", a.Subtitulo ?? string.Empty },
                    { "cuerpo", a.Cuerpo ?? string.Empty },
                    { "fecha", DateTime.TryParse(a.Fecha, out var dt) ? dt : (BsonValue)BsonNull.Value },
                    { "idResultado", idResultado },
                    { "fuente", new BsonDocument { { "url", f.Url ?? string.Empty }, { "tipo", f.Tipo ?? string.Empty }, { "nombre", f.Nombre ?? string.Empty } } }
                };
                Articulos.InsertOne(doc);
                var val = doc.GetValue("_id", BsonNull.Value);
                artId = val.IsBsonNull ? ObjectId.GenerateNewId().ToString() : val.ToString()!;
            }
            // attach to user found by idResultado
            // find user that has this Resultado
            var userFilter = Builders<BsonDocument>.Filter.Eq("Resultado._id", new ObjectId(idResultado));
            var update = Builders<BsonDocument>.Update.Push("articulos", new BsonDocument { { "idArticulo", artId }, { "descartado", false }, { "favorito", false } });
=======
            if (existing != null)
            {
                // Article already exists, do not insert anything, return false
                return false;
            }
            ObjectId artObjId;
            var doc = new BsonDocument
            {
                { "tema", a.Tema ?? string.Empty },
                { "titular", a.Titular ?? string.Empty },
                { "subtitulo", a.Subtitulo ?? string.Empty },
                { "cuerpo", a.Cuerpo ?? string.Empty },
                { "fecha", DateTime.TryParse(a.Fecha, out var dt) ? dt : (BsonValue)BsonNull.Value },
                { "idResultado", objIdResult },
                { "fuente", new BsonDocument { { "id", ObjectId.GenerateNewId() }, { "url", f.Url ?? string.Empty }, { "tipo", f.Tipo ?? string.Empty }, { "nombre", f.Nombre ?? string.Empty } } }
            };
            Articulos.InsertOne(doc);
            var val = doc.GetValue("_id", BsonNull.Value);
            if (!val.IsBsonNull && val.BsonType == BsonType.ObjectId) artObjId = val.AsObjectId;
            else if (!val.IsBsonNull && ObjectId.TryParse(val.ToString(), out var parsed2)) artObjId = parsed2;
            else artObjId = ObjectId.GenerateNewId();
            // Set the MongoId property on the Articulo object if available
            if (a != null)
            {
                a.MongoId = artObjId.ToString();
            }
            // Find the user by idResultado (in resultados array)
            var userFilter = Builders<BsonDocument>.Filter.ElemMatch("resultados", Builders<BsonDocument>.Filter.Eq("_id", objIdResult));
            var userDoc = Usuarios.Find(userFilter).FirstOrDefault();
            if (userDoc == null) return false;
            var userIdVal = userDoc.GetValue("_id", BsonNull.Value);
            if (userIdVal.IsBsonNull || userIdVal.BsonType != BsonType.ObjectId) return false;
            var userObjId = userIdVal.AsObjectId;
            // Push to articulos array with all required fields
            var articuloRef = new BsonDocument {
                { "idArticulo", artObjId },
                { "descartado", false },
                { "favorito", false }
            };
            var update = Builders<BsonDocument>.Update.Push("articulos", articuloRef);
>>>>>>> 58b0877 (mongo scraping)
            Usuarios.UpdateOne(userFilter, update);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public bool ToggleArticleFavorite(string articleId)
    {
        try
        {
            if (string.IsNullOrEmpty(articleId)) return false;
            // toggle favorito for all users that have this article in articulos array
<<<<<<< HEAD
            var filter = Builders<BsonDocument>.Filter.Eq("articulos.idArticulo", articleId);
=======
            // prefer comparing as ObjectId when possible
            ObjectId articleObjId;
            var filter = ObjectId.TryParse(articleId, out articleObjId)
                ? Builders<BsonDocument>.Filter.Eq("articulos.idArticulo", articleObjId)
                : Builders<BsonDocument>.Filter.Eq("articulos.idArticulo", articleId);
>>>>>>> 58b0877 (mongo scraping)
            var users = Usuarios.Find(filter).ToList();
            foreach (var u in users)
            {
                var uId = u.Contains("_id") ? u["_id"] : null;
                if (uId == null) continue;
                // find current value
                var arr = u.Contains("articulos") ? u["articulos"].AsBsonArray : null;
                if (arr == null) continue;
                for (int i = 0; i < arr.Count; i++)
                {
                    var item = arr[i].AsBsonDocument;
<<<<<<< HEAD
                    if (item.Contains("idArticulo") && item["idArticulo"].ToString() == articleId)
                    {
                        var current = item.Contains("favorito") ? item["favorito"].AsBoolean : false;
                        var arrayFilter = Builders<BsonDocument>.Filter.And(
                            Builders<BsonDocument>.Filter.Eq("_id", uId),
                            Builders<BsonDocument>.Filter.Eq("articulos.idArticulo", articleId)
                        );
=======
                    if (item.Contains("idArticulo"))
                    {
                        var match = false;
                        if (item["idArticulo"].BsonType == BsonType.ObjectId && ObjectId.TryParse(articleId, out var parsed))
                        {
                            match = item["idArticulo"].AsObjectId == parsed;
                        }
                        else
                        {
                            match = item["idArticulo"].ToString() == articleId;
                        }
                        if (!match) continue;
                        var current = item.Contains("favorito") ? item["favorito"].AsBoolean : false;
                        var arrayFilter = ObjectId.TryParse(articleId, out var parsedId)
                            ? Builders<BsonDocument>.Filter.And(
                                Builders<BsonDocument>.Filter.Eq("_id", uId),
                                Builders<BsonDocument>.Filter.Eq("articulos.idArticulo", parsedId)
                              )
                            : Builders<BsonDocument>.Filter.And(
                                Builders<BsonDocument>.Filter.Eq("_id", uId),
                                Builders<BsonDocument>.Filter.Eq("articulos.idArticulo", articleId)
                              );
>>>>>>> 58b0877 (mongo scraping)
                        var update = Builders<BsonDocument>.Update.Set("articulos.$.favorito", !current);
                        Usuarios.UpdateOne(arrayFilter, update);
                    }
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public void UpdateReadNotification(string notifId)
    {
        if (string.IsNullOrEmpty(notifId)) return;
        if (!ObjectId.TryParse(notifId, out ObjectId oid)) return;
        var filter = Builders<BsonDocument>.Filter.Eq("_id", oid);
        var doc = Notificaciones.Find(filter).FirstOrDefault();
        if (doc == null) return;
        var current = doc.Contains("leido") ? doc["leido"].AsBoolean : false;
        var update = Builders<BsonDocument>.Update.Set("leido", !current);
        Notificaciones.UpdateOne(filter, update);
    }

    public string ValidateLogin(string correo, string contraseña)
    {
        try
        {
            if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contraseña)) return string.Empty;
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("correo", correo),
                Builders<BsonDocument>.Filter.Eq("contraseña", contraseña)
            );
            var doc = Usuarios.Find(filter).FirstOrDefault();
            if (doc == null) return string.Empty;
            var val = doc.GetValue("_id", BsonNull.Value);
            return val.IsBsonNull ? string.Empty : val.ToString()!;
        }
        catch
        {
            return string.Empty;
        }
    }
    public bool Connected  { get;  private set;}
    private readonly MongoClient client;
    private readonly IMongoDatabase DB;
    private readonly IMongoCollection<BsonDocument> Articulos;
    private readonly IMongoCollection<BsonDocument> Usuarios;
    private readonly IMongoCollection<BsonDocument> Notificaciones;
}