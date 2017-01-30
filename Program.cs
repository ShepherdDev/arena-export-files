using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExportArenaFiles
{
    class Program
    {
        static IDbConnection Connection;

        static void Main( string[] args )
        {
            string serverName = "localhost";
            string databaseName = "ArenaDB";
            string exportFolder = ".\\";
            string connectionString;
            IDbCommand command;
            IDataReader reader;

            if ( args.Length > 0 )
            {
                exportFolder = args[0];

                if ( args.Length > 1 )
                {
                    serverName = args[1];

                    if ( args.Length > 2 )
                    {
                        databaseName = args[2];
                    }
                }
            }

            connectionString = string.Format( "Data Source={0}; Initial Catalog={1}; Integrated Security=SSPI; MultipleActiveResultSets=True;", serverName, databaseName );
            using ( Connection = new SqlConnection( connectionString ) )
            {
                Connection.Open();

                using ( command = Connection.CreateCommand() )
                {
                    command.CommandText = "SELECT folder_id,title FROM file_folder WHERE parent_folder_id = -1";
                    using ( reader = command.ExecuteReader() )
                    {
                        while ( reader.Read() )
                        {
                            ExportFolder( ( int ) reader[0], WebUtility.HtmlDecode( ( string ) reader[1] ), exportFolder );
                        }
                    }
                }
            }

            Console.WriteLine( "Finished" );
            Console.ReadKey();
        }

        static void ExportFolder( int folderId, string folderName, string exportFolder )
        {
            IDbCommand command;
            IDataReader reader;
            string targetPath;

            //
            // Create the folder.
            //
            targetPath = Path.Combine( exportFolder, SafeFileName( folderName ) );
            if ( !Directory.Exists( targetPath ) )
            {
                Directory.CreateDirectory( targetPath );
            }

            //
            // Process sub-folders.
            //
            using ( command = Connection.CreateCommand() )
            {
                command.CommandText = "SELECT folder_id,title FROM file_folder WHERE parent_folder_id = @folderId";
                command.Parameters.Add( new SqlParameter( "@folderId", folderId ) );
                using ( reader = command.ExecuteReader() )
                {
                    while ( reader.Read() )
                    {
                        ExportFolder( ( int ) reader[0], WebUtility.HtmlDecode( ( string ) reader[1] ), targetPath );
                    }
                }
            }

            //
            // Process files.
            //
            using ( command = Connection.CreateCommand() )
            {
                command.CommandText = "SELECT blob_id FROM file_folder_document WHERE document_folder_id = @folderId";
                command.Parameters.Add( new SqlParameter( "@folderId", folderId ) );
                using ( reader = command.ExecuteReader() )
                {
                    while ( reader.Read() )
                    {
                        ExportFile( ( int ) reader[0], targetPath );
                    }
                }
            }
        }

        static void ExportFile( int blobId, string exportFolder )
        {
            IDbCommand command;
            IDataReader reader;

            //
            // Process blob.
            //
            using ( command = Connection.CreateCommand() )
            {
                command.CommandText = "SELECT original_file_name,blob FROM util_blob WHERE blob_id = @blobId";
                command.Parameters.Add( new SqlParameter( "@blobId", blobId ) );
                using ( reader = command.ExecuteReader() )
                {
                    if ( reader.Read() )
                    {
                        string filename = Path.Combine( exportFolder, SafeFileName( ( string ) reader[0] ) );
                        File.WriteAllBytes( filename, ( byte[] ) reader[1] );

                        Console.WriteLine( string.Format( "Processed: {0}", filename ) );
                    }
                }
            }
        }

        static string SafeFileName( string filename )
        {
            string safeFilename = filename.Replace( '/', '_' ).Replace( '\\', '_' ).Replace( '?', '_' ).Replace( '*', '_' ).Replace( ':', '_' ).Replace( '|', '_' ).Replace( '"', '_' ).Replace( '<', '_' ).Replace( '>', '_' );

            while ( safeFilename.EndsWith( "." ) || safeFilename.EndsWith( " " ) )
            {
                safeFilename = safeFilename.Substring( 0, safeFilename.Length - 1 );
            }

            return safeFilename;
        }
    }
}
