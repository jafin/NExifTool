using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using NExifTool.Writer;

namespace NExifTool.Tests
{
    public class WriteTests
    {
        const string SrcFile = "space test.jpg";

        // japanese string taken from here: https://stackoverflow.com/questions/2627891/does-process-startinfo-arguments-support-a-utf-8-string
        //TODO : Put back utf8 test
        //const string Comment = "this is a これはテストです test";
        const string Comment = "this is a nexiftool test";
        const string CommentEncoded = "&#x11E;&#xDC;&#x15E;&#x130;&#xD6;&#xC7;&#x11F;&#xFC;&#x15F;i&#xF6;&#xE7;";

        // when writing the above encoded value manually via exiftool, it looks like it does some char replacements and results in the following:
        const string ExiftoolEncodedComment =
            "&#x11e;&Uuml;&#x15e;&#x130;&Ouml;&Ccedil;&#x11f;&uuml;&#x15f;i&ouml;&ccedil;";

        readonly List<Operation> _updates = new()
        {
            new SetOperation(new Tag("comment", Comment)),
            new SetOperation(new Tag("keywords", new[] { "first", "second", "third", "hello world" }))
        };

        readonly List<Operation> UPDATES_ENCODED = new()
        {
            new SetOperation(new Tag("comment", CommentEncoded)),
        };


        [Fact]
        public async void StreamToStreamWriteTest()
        {
            var opts = new ExifToolOptions();
            var et = new ExifTool(opts);
            var src = new FileStream(SrcFile, FileMode.Open);

            var result = await et.WriteTagsAsync(src, _updates);

            Assert.True(result.Success);
            Assert.NotNull(result.Output);

            ValidateTags(await et.GetTagsAsync(result.Output));
        }


        [Fact]
        public async void StreamToFileWriteTest()
        {
            var outputFile = "stream_to_file_test.jpg";
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            var opts = new ExifToolOptions();
            var et = new ExifTool(opts);

            await using var src = new FileStream(SrcFile, FileMode.Open);
            var result = await et.WriteTagsAsync(src, _updates, outputFile);

            Assert.True(result.Success);
            Assert.Null(result.Output);

            ValidateTags(await et.GetTagsAsync(outputFile));
            File.Delete(outputFile);
        }


        [Fact]
        public async void FileToStreamWriteTest()
        {
            var opts = new ExifToolOptions();
            var et = new ExifTool(opts);

            var result = await et.WriteTagsAsync(SrcFile, _updates);

            Assert.True(result.Success);
            Assert.NotNull(result.Output);

            ValidateTags(await et.GetTagsAsync(result.Output));
        }


        [Fact]
        public async void FileToFileWriteTest()
        {
            var opts = new ExifToolOptions();
            var et = new ExifTool(opts);
            var destinationFile = "file_to_file_test.jpg";

            if (File.Exists(destinationFile))
            {
                File.Delete(destinationFile);
            }

            var result = await et.WriteTagsAsync(SrcFile, _updates, destinationFile);

            //Assert.Null(result.ErrorMessage);
            Assert.True(result.Success);
            Assert.Null(result.Output);

            ValidateTags(await et.GetTagsAsync("file_to_file_test.jpg"));

            File.Delete(destinationFile);
        }


        [Fact]
        public async void FileToFileWriteTestEncoded()
        {
            var opts = new ExifToolOptions()
            {
                EscapeTagValues = true
            };

            var et = new ExifTool(opts);

            var result = await et.WriteTagsAsync(SrcFile, UPDATES_ENCODED, "file_to_file_encoded_test.jpg");

            Assert.True(result.Success);
            Assert.Null(result.Output);

            ValidateEncodedTag(await et.GetTagsAsync("file_to_file_encoded_test.jpg"));

            File.Delete("file_to_file_encoded_test.jpg");
        }

        [Fact]
        public async void OverwriteTest()
        {
            File.Copy(SrcFile, "overwrite_test.jpg", true);

            var opts = new ExifToolOptions();
            var et = new ExifTool(opts);

            var result = await et.OverwriteTagsAsync("overwrite_test.jpg", _updates, FileWriteMode.OverwriteOriginal);

            Assert.True(result.Success);
            Assert.Null(result.Output);

            ValidateTags(await et.GetTagsAsync("overwrite_test.jpg"));

            File.Delete("overwrite_test.jpg");
        }


        [Fact]
        public async void OverwriteOriginalInPlaceTest()
        {
            File.Copy(SrcFile, "overwrite_original_in_place_test.jpg", true);

            var opts = new ExifToolOptions();
            var et = new ExifTool(opts);

            var result = await et.OverwriteTagsAsync("overwrite_original_in_place_test.jpg", _updates,
                FileWriteMode.OverwriteOriginalInPlace);

            Assert.True(result.Success);
            Assert.Null(result.Output);

            ValidateTags(await et.GetTagsAsync("overwrite_original_in_place_test.jpg"));

            File.Delete("overwrite_original_in_place_test.jpg");
        }


        void ValidateTags(IEnumerable<Tag> tags)
        {
            var enumerable = tags.ToList();
            var commentTag =
                enumerable.SingleOrDefault(x => string.Equals(x.Name, "comment", StringComparison.OrdinalIgnoreCase));
            var keywordsTag = enumerable.SinglePrimaryTag("keywords");

            Assert.NotNull(commentTag);
            Assert.Equal(Comment.Replace("\"", string.Empty), commentTag.Value);

            Assert.NotNull(keywordsTag);
            Assert.Equal(4, keywordsTag.List.Count);
            Assert.Equal("first", keywordsTag.List[0]);
            Assert.Equal("hello world", keywordsTag.List[3]);
        }

        void ValidateEncodedTag(IEnumerable<Tag> tags)
        {
            var commentTag =
                tags.SingleOrDefault(x => string.Equals(x.Name, "comment", StringComparison.OrdinalIgnoreCase));

            Assert.NotNull(commentTag);
            Assert.Equal(ExiftoolEncodedComment.Replace("\"", string.Empty), commentTag.Value);
        }
    }
}