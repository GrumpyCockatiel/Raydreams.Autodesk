using Newtonsoft.Json;
using Raydreams.Autodesk.CLI.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Raydreams.Autodesk.CLI.IO
{
	/// <summary>Defines how to read and write tokens physically from the file system</summary>
	public interface ITokenIO
	{
		/// <summary>Read a token</summary>
		/// <param name="path"></param>
		/// <returns></returns>
		Auth0Token? ReadToken( string path );

		/// <summary>Write a token</summary>
		Task WriteToken( Auth0Token token, string path );
	}

    /// <summary>Reads and writes the legacy token format to a file encrypted</summary>
    public class MemoryTokenIO : ITokenIO
    {
        private Auth0Token? _token = null;

        public Auth0Token? ReadToken( string path )
        {
            return this._token;
        }

        public Task WriteToken( Auth0Token token, string path )
        {
            this._token = token;

            return Task.CompletedTask;
        }
    }

    /// <summary>Reads and writes the legacy token format to a file encrypted</summary>
    public class FileTokenIO : ITokenIO
    {
        /// <summary>When true will encrypt the file with the legacy AES256 encyption, false will write as plain text</summary>
        public bool Encrypted { get; set; } = false;

        /// <summary>Read the token from the full file path</summary>
        /// <remarks>This cant be async</remarks>
        public Auth0Token? ReadToken(string path)
        {
            Auth0Token? token = null;

            if (String.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return token;

            // read the token file
            string json = File.ReadAllText(path);

            return JsonConvert.DeserializeObject<Auth0Token>(json);
        }

        /// <summary>Write a token to the file system</summary>
        /// <param name="path">The full file path to write to which may not already exist.</param>
        /// <remarks></remarks>
        public async Task WriteToken(Auth0Token token, string path)
        {
            if (String.IsNullOrWhiteSpace(path) || token == null)
                return;

            // get just the directory to write the token file to
            DirectoryInfo? dir = new FileInfo(path).Directory;

            // create the parent folder if it does not exist
            if ( dir != null && !dir.Exists)
                dir.Create();

            // serialize and encrypt the file
            string json = JsonConvert.SerializeObject(token);

            using StreamWriter writer = File.CreateText(path);
            await writer.WriteAsync(json);
        }
    }
}
