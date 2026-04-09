using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Ardalis.GuardClauses;

namespace Uccs.Fair;

internal static class ProductFieldsUtils
{
	public static IEnumerable<FieldValueModel>? GetMappedFields([NotNull] Product product, [NotNull] Publication publication)
	{
		Guard.Against.Null(product);
		Guard.Against.Null(publication);

		return GetMappedFieldsVersion(product, publication.ProductVersion);
	}

	public static IEnumerable<FieldValueModel>? GetLatestMappedFields([NotNull] Product product)
	{
		Guard.Against.Null(product);

		FieldValue[] fields = product.Versions.LastOrDefault()?.Fields;
		if(fields == null)
		{
			return null;
		}

		Field[] declaration = Product.FindDeclaration(product.Type);
		return MapValues(declaration, fields);
	}

	public static IEnumerable<FieldValueModel>? GetMappedFieldsVersion([NotNull] Product product, [NonNegativeValue] int fieldsVersion)
	{
		Guard.Against.Null(product);
		Guard.Against.Negative(fieldsVersion);

		FieldValue[] fields = product.Versions.FirstOrDefault(i => i.Id == fieldsVersion)?.Fields;
		if(fields == null)
		{
			return null;
		}

		Field[] declaration = Product.FindDeclaration(product.Type);
		return MapValues(declaration, fields);
	}

	static IEnumerable<FieldValueModel> MapValues(Field[] declarationFields, FieldValue[] productFields)
	{
		var result = new List<FieldValueModel>();

		foreach(FieldValue value in productFields)
		{
			var declarationField = declarationFields.FirstOrDefault(d => d.Name == value.Name);
			if(declarationField == null)
				continue;

			var model = new FieldValueModel
			{
				Name = value.Name,
				Type = declarationField?.Type,
				Value = ConvertValue(declarationField?.Type, value),
				Children = value.Fields != null && value.Fields.Length > 0
					? MapValues(declarationField?.Fields ?? [], value.Fields)
					: null
			};

			result.Add(model);
		}

		return result;
	}

	static object ConvertValue(FieldType? declarationType, FieldValue field)
	{
		if(field?.Value == null)
			return null;

		switch(declarationType)
		{
			case FieldType.Integer:
				return BinaryPrimitives.ReadInt32LittleEndian(field.Value);
			case FieldType.Float:
				return BinaryPrimitives.ReadDoubleLittleEndian(field.Value);

			case FieldType.TextUtf8:
			case FieldType.StringUtf8:
			case FieldType.URI:
			case FieldType.URL:
			case FieldType.LanguageCode:
			case FieldType.License:
			case FieldType.DistributionType:
			case FieldType.Platform:
			case FieldType.OS:
			case FieldType.CPUArchitecture:
			case FieldType.Hash:
				return field.AsUtf8;

			case FieldType.StringAnsi:
				return Encoding.Default.GetString(field.Value);

			case FieldType.Money:
				return BinaryPrimitives.ReadInt64LittleEndian(field.Value);

			case FieldType.FileId:
				return field.AsAutoId.ToString();

			case FieldType.Date:
				return BinaryPrimitives.ReadInt32LittleEndian(field.Value);
		}

		return null;
	}
}
