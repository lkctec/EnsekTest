import React, { useState, ChangeEvent, FormEvent } from 'react';
import { uploadFile } from '../services/fileUploadService';

interface FileUploadFormProps {
  endpoint: string;
}

const FileUploadForm: React.FC<FileUploadFormProps> = ({ endpoint }) => {
  const [file, setFile] = useState<File | null>(null);
  const [uploading, setUploading] = useState<boolean>(false);
  const [message, setMessage] = useState<string>('');

  const handleFileChange = (e: ChangeEvent<HTMLInputElement>) => {
    const selectedFile = e.target.files?.[0] || null;
    setFile(selectedFile);
    setMessage('');
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!file) {
      setMessage('Please select a CSV file to upload.');
      return;
    }
    if (!endpoint) {
      setMessage('Please ensure the endpoint is set.');
      return;
    }
    setUploading(true);
    setMessage('');
    
    const result = await uploadFile(file, endpoint);
    setMessage(result.message);
    setUploading(false);
  };

  return (
    <form onSubmit={handleSubmit}>
      <div style={{ marginTop: '20px' }}>
        <input
          type="file"
          accept=".csv"
          onChange={handleFileChange}
        />
        {file && (
          <p style={{ marginTop: '10px', fontSize: '0.9em', color: '#666' }}>
            Selected file: <strong>{file.name}</strong>
          </p>
        )}
      </div>
      <button type="submit" disabled={uploading} style={{ marginTop: '20px' }}>
        {uploading ? 'Uploading...' : 'Upload CSV'}
      </button>
      {message && <p style={{ marginTop: '20px', color: 'black' }}>{message}</p>}
    </form>
  );
};

export default FileUploadForm;
