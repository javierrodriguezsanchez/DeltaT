namespace DeltaTEngine;
using System.Collections;

//Esta clase encapsula un mensaje
public class messege
//Esta clase encapsula un mensaje
{
    public string text{get;}
    //texto de dicho mensaje

    public DateTime moment;
    //hora a la que fue enviado

    public messege(string text)//constructor
    {
        this.text=text;
        moment=DateTime.Now;
    }

    private messege(string text, string time)
    //constructor privado, permite obtener un mensaje que fue enviado con antelacion
    {
        this.text=text;
        moment=DateTime.Parse(time);
    }

    public static messege Parse(string text)
    //crea un mensaje a partir de un texto
    {
        string[] split=text.Split('\0');
        //nota: los mensajes se escriben separando la hora del texto mediante un caracter nulo
        return new messege(split[1],split[0]);
    }

    public override string ToString() => moment+"\0"+text;
    //notar la estructura
}

public class user//agregar cara
{
    //informacion personal del usuario
    //--------------------------------------------
    public string name{get; private set;}
    private string password;
    public int age{get; private set;}
    public string From{get; private set;}
    public string sex{get; private set;}
    public string[] likes{get; private set;}
    //notar que al usuario le puede gustar mas de un tipo de persona
    public string info{get; private set;}
    public string lookingFor{get; private set;}
    //--------------------------------------------

    public user(string name, string password,int age,string From,string sex,string[] likes,string info,string lookingFor)
    //constructor
    {
        this.name=name;
        this.age=age;
        this.sex=sex;
        this.From=From;
        this.password=password;
        this.likes=likes;
        this.info=info;
        this.lookingFor=lookingFor;
    }

    public bool CheckPassword(string usertry) => usertry==password;
    //retorna true si el texto introducido es igual a la contrasena

    public bool ChangeData(string password, user newDatas)
    //permite cambiar la informacion por la del usuario de entrada
    //requiere contrasenna
    {
        if(CheckPassword(password))
           return false;
        this.age=newDatas.age;
        this.From=From;
        this.info=newDatas.info;
        this.likes=newDatas.likes;
        this.lookingFor=newDatas.lookingFor;
        this.name=newDatas.name;
        this.sex=newDatas.sex;
        this.password=newDatas.password;
        return true;
    }
    
    public override string ToString()
    //fijarse bien en como se guarda
    //en info y en locking for, como pueden introducirse saltos de linea se ponen \0
    //para denotar cdo comienza y termina
    {
        string retorno=name+"\n";
        retorno+=password+"\n";
        retorno+=age+"\n";
        retorno+=From+"\n";
        retorno+=sex+"\n";
        foreach (var item in likes)
        {
            retorno+=item+" ";
        }
        retorno=retorno.Remove(retorno.Length-1)+"\n";
        retorno+="\0"+info+"\n";
        retorno+="\0"+lookingFor+"\n";
        return retorno;
    }
    public static user Parse(string text)
    //permite obtener un perfil a partir de un texto
    {
        //separando info y lockingfor del resto
        string[] split=text.Split('\0');
        string info=split[1];
        string lookingFor=split[2];

        //separando el resto
        split=split[0].Split('\n');
        
        return new user(split[0],split[1],int.Parse(split[2]),split[3],split[4],
        split[5].Split(' ')/*obteniendo los gustos*/,info,lookingFor);
    }
}

public class conversation:IEnumerable<(messege,bool)>
//maneja la conversacion. Se puede ver como una coleccion de mensajes ordenado x hora enviada
//el bool indica si el mensaje fue enviado o recibido
//los mensajes se guardan en Database/Conversation/{nombre del remitente}/{nombre del destinatario}.txt
//notar que la conversacion esta dividida en 2
{
    public user owner{get;}//remitente
    public user contact{get;}//destinatario
    public DateTime timeCreated;
    public conversation(user u1, user u2)//constructor
    {
        owner=u1;
        contact=u2;
        timeCreated=DateTime.Now;
    }

    public void WriteMessege(string newMesegge)//permite escribir un mensaje
    {
        messege ToSend=new(newMesegge);//crea el mensaje

        StreamReader Text=new(Path.Combine(Bridge.Database,"Conversations",owner.name,contact.name+".txt"));
        //busca donde guardarlo
        
        string Actualization=ToSend+"\n\0\n"+Text.ReadToEnd();
        //notar que se adiciona el mensaje separado por una linea que contiene al caracter nulo
        //esto es lo que diferencia de un mensaje a otro
        //notar tambien que el primer mensaje del texto es el ultimo en ser enviado

        //ahora vuelve a abrir el archivo y reemplaza el texto con la actualizacion
        Text.Close();
        StreamWriter TheNew=new(Path.Combine(Bridge.Database,"Conversations",owner.name,contact.name+".txt"));
        TheNew.WriteLine(Actualization);
        TheNew.Close();
        
    }

    private IEnumerator<messege> SendedMesseges()
    //devuelve todos los mensajes enviados ordenados por fecha
    {
        StreamReader reader=new(Path.Combine(Bridge.Database,"Conversations",
                                owner.name,contact.name+".txt"));
        //direccion de donde esta
        
        string new_messege="";
        while(reader.EndOfStream)
        {
            string newLine=reader.ReadLine()!;
            //va guardando el mensaje

            if(newLine=="\0")//fin del mensaje, lo devuelve y vacia el mensaje
            {
                yield return messege.Parse(new_messege);
                new_messege="";
                continue;
            }
            //de lo contrario le adiciona una nueva linea
            new_messege+=newLine;
        }
        reader.Close();
    } 
    private IEnumerator<messege> RecievedMesseges()
    //la estructura es practicamente la misma, pero para los mensajes recibidos
    {
        StreamReader reader=new(Path.Combine(Bridge.Database,"Conversations",
                                owner.name,contact.name+".txt"));
        string new_messege="";
        while(reader.EndOfStream)
        {
            string newLine=reader.ReadLine()!;
            if(newLine=="\0")
            {
                yield return messege.Parse(new_messege);
                new_messege="";
                continue;
            }
            new_messege+=newLine;
        }
        reader.Close();
    }

    public IEnumerator<(messege,bool)> GetEnumerator()
    //mezcla los enviados con los recibidos
    {
        //obtenemos los mensajes enviados y recibidos
        IEnumerator<messege> sended=SendedMesseges();
        IEnumerator<messege> recieved=RecievedMesseges();
        
        bool EndOfTheSended=sended.Current is null;//permite saber si se acabaron los mensajes enviados

        while(!EndOfTheSended && recieved.Current is not null)
        //este ciclo se ejecuta mientras hayan tanto mensajes enviados como recibidos
        {
            if(sended.Current!.moment.CompareTo(recieved.Current.moment)>0)
            //si el enviado es el ultimo devuelvelo
            {
                yield return (sended.Current,true);
                if(!sended.MoveNext())
                //pasa para el siguiente, si se acabaron termina el ciclo
                {
                    EndOfTheSended=true;
                    break;
                }
                //continua con el ciclo
                continue;
            }
            
            //si el ultimo mensaje fue recibido devuelvelo
            yield return (recieved.Current,false);
            
            if(!recieved.MoveNext()) break;
            //pasa para el siguiente, si se acabaron termina el ciclo
                
        }
        if(EndOfTheSended)
        //si ya no se enviaron mas mensajes
        {
            while(recieved.Current is not null)//devuelve el resto de recibidos
            {
                yield return (recieved.Current,false);
                recieved.MoveNext();
            }
        }
        else
        //solo se ejecuta si se acabaron primero los mensajes recibidos
        {
            while(sended.Current is not null)//envia lo que queda
            {
                yield return (sended.Current,true);
                sended.MoveNext();
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class acount
//indica la cuenta de un usuario y todo lo relativo a este
{
    //Todo lo que importa en una cuenta es el usuario, los posibles 
    //matches, quienes son los que le gusta y sus conversaciones
    //-------------------------------------------------------------
    public user owner{get;}
    public List<conversation> chats{get;}
    public List<user> PosibleMatches;
    public List<user> Matches;
    //-------------------------------------------------------------


    public acount(user owner, bool Start=true)
    //constructor. Start dice si el usuario esta o no en la base de datos
    {
        this.owner=owner;
        chats=new();
        PosibleMatches=new();
        Matches=new();

        //dos funciones descritas mas adelante
        if(Start)CreateProfile();
        Actualize();
    }

    private void CreateProfile()
    //adiciona al usuario a la base de datos
    {
        StreamWriter writer=new(Path.Combine(Bridge.Database,"Users",
                                            owner.sex,owner.name+".txt"));
        writer.WriteLine(owner);
        writer.Close();
        //Guarda su informacion personal en el sitio correspondiente
        
        Directory.CreateDirectory(Path.Combine(Bridge.Database,"Conversations",owner.name));
        //crea una carpeta donde guarda los mensajes que enviara
    }

    //funciones encargadas de actualizar la informacion
    //-------------------------------------------------
    public void Actualize()
    //Esta funcion actualiza toda la informacion de una cuenta
    {
        ActualizeMatches();
        ActualizeChats();
    }
    private void ActualizeMatches()
    //actualiza la lista de personas que te pudieran interesar
    {
        PosibleMatches.Clear();
        foreach (var sex in owner.likes)
        //recorre cada sexo que le gusta al usuario
        {
            string[] names=Directory.GetFiles(Path.Combine(Bridge.Database,"Users",sex));
            //obtiene todos los nombres de las personas de dicho sexo
            foreach (var dir in names)
            {
                if(Path.GetFileNameWithoutExtension(dir)==owner.name)
                    continue;//quita el caso que te salgas tu mismo

                //Obtiene la info de cada persona y comprueba que estes entre sus gustos
                StreamReader reader=new(dir);
                user person=user.Parse(reader.ReadToEnd());
                foreach (var item in person.likes)
                    if(item==owner.sex)
                    {
                        PosibleMatches.Add(person);//si son compatibles se adiciona
                        break;
                    }
                reader.Close();                
            }
        }

    }
    private void ActualizeChats()
    //actualiza tus chats, bloquea a quien quisiste bloquear y te bloquean si te bloquearon
    //ademas inicia conversaciones con las personas adicionadas
    {
        bool[] ChatExist=new bool[Matches.Count];
        //indica con quienes de tus macheados tienes conversaciones
        bool[] TheOtherMatch=new bool[chats.Count];
        //indica si no has sido bloqueado en una de tus conversaciones

        //obtiendo los valores de las listas anteriores
        for(int i=0;i<Matches.Count;i++)
            for(int j=0;j<chats.Count;j++)
            {
                if(Matches[i].name==chats[j].contact.name)
                {
                    ChatExist[i]=true;
                    TheOtherMatch[j]=File.Exists(Path.Combine(Bridge.Database,"Conversations",
                                                Matches[i].name,owner.name+".txt"));
                }
            }

        //comprueba dentro de las conversaciones que no estan si has sido macheado
        //y en ese caso inicia una nueva conversacion
        for(int i=0;i<ChatExist.Length;i++)
            if(!ChatExist[i] && File.Exists(Path.Combine(Bridge.Database,"Conversations",Matches[i].name,owner.name+".txt")))
                chats.Add(new(owner,Matches[i]));
                
        
        //elimina esas conversaciones donde fuiste bloqueado
        for(int j=0,k=0;j<TheOtherMatch.Length;j++)
            if(!TheOtherMatch[j])
                chats.RemoveAt(j-k++);
    }
    //-------------------------------------------------

    public user LookProfile(string name)
    //devuelve la informacion de otro usuario o null si no esta
    {
        foreach(var item in PosibleMatches)
            if(item.name==name)
                return item;
        Actualize();
        return null!;
    }

    public bool ModifyPerfil(string OldPassword, user newUser)
    //modifica la informacion del usuario
    {
        //guardando la informacion anterior
        string oldSex=owner.sex;
        string oldName=owner.name;

        //Intento de modificar, si falla es contrasena incorrecta
        if(!owner.ChangeData(OldPassword,newUser))
            return false;

        //Eliminando el antiguo perfil y creando uno nuevo 
        File.Delete(Path.Combine(Bridge.Database,"Users",oldSex,oldName+".txt"));
        CreateProfile();

        //si no se cambio el nombre todo esta bien
        if(owner.name==oldName)
            return true;
        
        //Aqui la cosa se pone fea :(
        
        //aqui creamos nuevos archivos de los mensajes recibidos y se los ponemos al nuevo nombre
        foreach (var item in chats)
        {
            if(File.Exists(Path.Combine(Bridge.Database,"Conversations",item.contact.name,oldName+".txt")))
            {
                File.Copy(Path.Combine(Bridge.Database,"Conversations",item.contact.name,oldName+".txt"),
                    Path.Combine(Bridge.Database,"Conversations",item.contact.name,owner.name+".txt"));
                File.Delete(Path.Combine(Bridge.Database,"Conversations",item.contact.name,oldName+".txt"));
            }
        }

        //aqui movemos todos los mensajes enviados a una nueva carpeta
        Directory.CreateDirectory(Path.Combine(Bridge.Database,"Conversations",owner.name));
        foreach(var item in chats)
            File.Copy(Path.Combine(Bridge.Database,"Conversations",oldName,item.contact.name+".txt"),
                Path.Combine(Bridge.Database,"Conversations",owner.name,item.contact.name+".txt"));
        Directory.Delete(Path.Combine(Bridge.Database,"Conversations",oldName));
        
        Actualize();
        return true;
    }

    public bool SendMessege(string name,string text)
    {
        if(!File.Exists(Path.Combine(Bridge.Database,"Conversations",name,owner.name+".txt")))
        {
            Actualize();
            if(File.Exists(Path.Combine(Bridge.Database,"Conversations",name,owner.name+".txt")))
                return false;
        }
        foreach (var item in chats)
        {
            if(item.contact.name==name)
            {
                item.WriteMessege(text);
                return true;
            }
        }
        return false;
    }

    //Funciones encargadas de machear y desmachear gente
    //--------------------------------------------------
    public bool Match(string name)
    //retorna true si la persona fue macheada con exito
    //De machearse, se crea un archivo de conversacion con esa persona
    {
        foreach(var item in PosibleMatches)
            if(item.name==name)
            {
                Matches.Add(item);
                File.Create(Path.Combine(Bridge.Database,"Conversations",owner.name,name+".txt"));
                Actualize();
                return true;
            }
        return false;    
    }
    public bool UnMatch(string name)
    //bloquea a alguien. Eliminando el archivo de los mensajes enviados
    {
        foreach(var item in PosibleMatches)
            if(item.name==name)
            {
                Matches.Remove(item);
                File.Delete(Path.Combine(Bridge.Database,"Conversations",owner.name,name+".txt"));
                Actualize();
                return true;
            }
        return false;
    }
    //--------------------------------------------------
}