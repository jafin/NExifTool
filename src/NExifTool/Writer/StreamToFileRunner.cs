using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Medallion.Shell;


namespace NExifTool.Writer
{
    public class StreamToFileRunner
        : Runner
    {
        readonly Stream _src;
        readonly string _dst;


        public StreamToFileRunner(ExifToolOptions opts, Stream src, string dst)
            : base(opts)
        {
            _src = src ?? throw new ArgumentNullException(nameof(src));
            _dst = dst ?? throw new ArgumentNullException(nameof(dst));
        }


        public override async Task<WriteResult> RunProcessAsync(IEnumerable<Operation> updates)
        {
            GetUpdateArgs(updates);
            var runner = new StreamToStreamRunner(_options, _src);
            var result = await runner.RunProcessAsync(updates).ConfigureAwait(false);

            if (result.Success)
            {
                using var destinationStream = new FileStream(_dst, FileMode.CreateNew, FileAccess.ReadWrite);
                await result.Output.CopyToAsync(destinationStream)
                    .ConfigureAwait(false);

                return new WriteResult(true, null);
            }

            return new WriteResult(false, null);
        }
    }
}